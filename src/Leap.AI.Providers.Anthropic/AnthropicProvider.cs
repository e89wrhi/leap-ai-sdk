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
/// Anthropic Messages API implementation for Leap AI.
/// <para>
/// I've designed this provider to handle the unique nuances of Anthropic's Messages API,
/// which differs significantly from OpenAI-style schemas (especially in how it handles system prompts
/// and the strict separation of message roles). 
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
        
        // I set these headers once to ensure every request is properly identified
        // We set these headers to ensure every request is properly identified
        // by Anthropic's load balancers and versioned correctly.
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", options.ApiKey);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", options.AnthropicVersion);
        if (!_httpClient.DefaultRequestHeaders.Accept.Any(x => x.MediaType == "application/json"))
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    /// <summary>
    /// Executes a non-streaming chat request.
    /// I've optimized this to wait for the full JSON response and then map the content
    /// blocks into the unified Leap ChatResponse structure.
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
    /// Implements SSE streaming for Anthropic.
    /// Anthropic's stream format is "event-based" (message_start, content_block_delta, etc.),
    /// so 1 wrote this state machine to collect tokens and correctly yield them as ChatChunks.
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
        // Anthropic requires system prompts to be a separate top-level field,
        // so I filter them out of the main message list.
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
        
        // I map Anthropic's specific status codes to Leap's unified exception types
        // so developers don't have to catch provider-specific errors.
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
