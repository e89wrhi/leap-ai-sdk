using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;
using Leap.AI.Core.Models;
using Leap.AI.Providers.Anthropic.Dtos;

namespace Leap.AI.Providers.Anthropic;

/// <summary>
/// Leap AI provider for the Anthropic Messages API (Claude 3/3.5/3.7 family).
/// <para>
/// Handles the unique nuances of Anthropic's Messages API, which differs from
/// OpenAI-style schemas — particularly around system prompts (a separate top-level
/// field) and the strict alternation of message roles.
/// </para>
/// </summary>
public sealed class AnthropicProvider : ILeapProvider
{
    private readonly AnthropicOptions _options;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public string Name => "anthropic";

    public AnthropicProvider(AnthropicOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = options.HttpClient ?? new HttpClient();

        // Set once so every request is properly identified by Anthropic's
        // load balancers and pinned to the correct API version.
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", options.ApiKey);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", options.AnthropicVersion);
        if (!_httpClient.DefaultRequestHeaders.Accept.Any(x => x.MediaType == "application/json"))
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    /// <summary>
    /// Executes a non-streaming chat request and maps the full Anthropic
    /// response body into the unified <see cref="ChatResponse"/> structure.
    /// </summary>
    public async Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken ct = default)
    {
        var anthropicRequest = MapRequest(request, isStream: false);
        var content = new StringContent(JsonSerializer.Serialize(anthropicRequest, _jsonOptions), Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/messages")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(httpRequest, ct);
        await EnsureSuccessAsync(response, ct);

        var json = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<AnthropicChatResponse>(json, _jsonOptions);

        return MapResponse(dto!, request);
    }

    /// <summary>
    /// Streams response chunks via Anthropic's SSE event format
    /// (<c>message_start</c>, <c>content_block_delta</c>, <c>message_stop</c>, etc.)
    /// and yields each token as a <see cref="ChatChunk"/>.
    /// </summary>
    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var anthropicRequest = MapRequest(request, isStream: true);
        var postData = JsonSerializer.Serialize(anthropicRequest, _jsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/messages")
        {
            Content = new StringContent(postData, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccessAsync(response, ct);

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Anthropic stream format: 
            // event: status
            // data: {"type": "..."}
            if (line.StartsWith("data: "))
            {
                var data = line["data: ".Length..];
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;
                var type = root.GetProperty("type").GetString();

                switch (type)
                {
                    case "content_block_delta":
                        var delta = root.GetProperty("delta");
                        if (delta.TryGetProperty("text", out var textPipe))
                        {
                            yield return new ChatChunk { Text = textPipe.GetString() ?? "" };
                        }
                        break;

                    case "message_stop":
                        yield return new ChatChunk { IsComplete = true };
                        break;
                }
            }
        }
    }

    private AnthropicChatRequest MapRequest(ChatRequest request, bool isStream)
    {
        // Anthropic requires system prompts in a separate top-level field —
        // they must be excluded from the messages list.
        var systemMessage = request.Messages.LastOrDefault(m => m.Role == "system");
        var messages = request.Messages
            .Where(m => m.Role != "system")
            .Select(m => new AnthropicMessage 
            { 
                Role = m.Role == "assistant" ? "assistant" : "user", 
                Content = m.Content 
            })
            .ToList();

        return new AnthropicChatRequest
        {
            Model = request.AdditionalProperties?.TryGetValue("model", out var m) == true ? m.ToString()! : _options.DefaultModel,
            Messages = messages,
            System = systemMessage?.Content,
            MaxTokens = request.MaxTokens ?? 4096,
            Temperature = request.Temperature,
            Stream = isStream
        };
    }

    private ChatResponse MapResponse(AnthropicChatResponse dto, ChatRequest originalRequest)
    {
        return new ChatResponse
        {
            Id = dto.Id,
            Model = dto.Model,
            Provider = "anthropic",
            Text = string.Join("", dto.Content.Select(c => c.Text)),
            FinishReason = dto.StopReason ?? "end_turn",
            Usage = dto.Usage != null ? new LeapUsage
            {
                PromptTokens = dto.Usage.InputTokens,
                CompletionTokens = dto.Usage.OutputTokens,
                TotalTokens = dto.Usage.InputTokens + dto.Usage.OutputTokens
            } : null
        };
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);

        // Map Anthropic status codes to Leap's unified exception types
        // so callers never need to handle provider-specific error shapes.
        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden 
                => new LeapAuthException($"Anthropic authentication failed: {body}", "anthropic"),
            
            System.Net.HttpStatusCode.TooManyRequests 
                => new LeapRateLimitException($"Anthropic rate limit reached: {body}", "anthropic"),
            
            _ => new LeapException($"Anthropic API error ({(int)response.StatusCode}): {body}", response.StatusCode, "anthropic")
        };
    }
}
