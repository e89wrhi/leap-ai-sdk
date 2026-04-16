using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;
using Leap.AI.Core.Models;
using Leap.AI.Providers.Google.Internal;

namespace Leap.AI.Providers.Google;

/// <summary>
/// Leap AI provider for Google's Gemini models via the Generative Language REST API.
/// Supports gemini-1.5-flash, gemini-1.5-pro, and gemini-2.0-flash with full streaming.
/// </summary>
/// <remarks>
/// Gemini's API differs from OpenAI-style APIs in two important ways:
/// <list type="bullet">
///   <item>System prompts are a separate <c>systemInstruction</c> top-level field, not a message.</item>
///   <item>Roles are <c>user</c> and <c>model</c> (not <c>assistant</c>).</item>
/// </list>
/// This provider handles that mapping transparently.
/// </remarks>
public sealed class GoogleProvider : ILeapProvider
{
    private readonly GoogleOptions _options;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public string Name => "google";

    public GoogleProvider(GoogleOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = options.HttpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public async Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken ct = default)
    {
        var url  = BuildUrl(isStream: false);
        var body = BuildBody(request);
        var json = JsonSerializer.Serialize(body, _jsonOptions);

        using var response = await _httpClient.PostAsync(
            url, new StringContent(json, Encoding.UTF8, "application/json"), ct);

        await EnsureSuccessAsync(response, ct);

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<GeminiResponse>(responseJson, _jsonOptions) ?? new GeminiResponse();

        return MapResponse(dto);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var url  = BuildUrl(isStream: true);
        var body = BuildBody(request);
        var json = JsonSerializer.Serialize(body, _jsonOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        await EnsureSuccessAsync(response, ct);

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            GeminiStreamChunk? chunk;
            try   { chunk = JsonSerializer.Deserialize<GeminiStreamChunk>(data, _jsonOptions); }
            catch { continue; }

            if (chunk is null) continue;

            var candidate = chunk.Candidates.FirstOrDefault();
            var text      = candidate?.Content?.Parts.FirstOrDefault()?.Text ?? string.Empty;
            var isDone    = candidate?.FinishReason is not null;

            yield return new ChatChunk
            {
                Text         = text,
                IsComplete   = isDone,
                FinishReason = candidate?.FinishReason,
                Usage = isDone && chunk.UsageMetadata is { } u ? new LeapUsage
                {
                    PromptTokens     = u.PromptTokenCount,
                    CompletionTokens = u.CandidatesTokenCount,
                    TotalTokens      = u.TotalTokenCount,
                } : null,
            };
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private string BuildUrl(bool isStream)
    {
        var action = isStream ? "streamGenerateContent?alt=sse" : "generateContent";
        return $"{_options.BaseUrl.TrimEnd('/')}/models/{_options.DefaultModel}:{action}&key={_options.ApiKey}";
    }

    /// <summary>
    /// Converts the unified <see cref="ChatRequest"/> into Gemini's request format.
    /// Extracts the system prompt into its own field and maps role names correctly.
    /// </summary>
    private GeminiRequest BuildBody(ChatRequest request)
    {
        var systemMsg = request.Messages.LastOrDefault(m => m.Role == "system");

        var contents = request.Messages
            .Where(m => m.Role != "system")
            .Select(m => new GeminiContent
            {
                // Gemini uses "model" where other providers use "assistant"
                Role  = m.Role == "assistant" ? "model" : "user",
                Parts = [new GeminiPart { Text = m.Content }]
            })
            .ToList();

        return new GeminiRequest
        {
            Contents         = contents,
            SystemInstruction = systemMsg is not null
                ? new GeminiContent { Parts = [new GeminiPart { Text = systemMsg.Content }] }
                : null,
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature    = request.Temperature,
                MaxOutputTokens = request.MaxTokens,
                ResponseMimeType = request.ResponseFormat is not null ? "application/json" : null,
            },
        };
    }

    private ChatResponse MapResponse(GeminiResponse dto)
    {
        var candidate = dto.Candidates.FirstOrDefault();
        var text = string.Join("", candidate?.Content?.Parts.Select(p => p.Text ?? "") ?? []);

        return new ChatResponse
        {
            Id           = Guid.NewGuid().ToString(),
            Model        = _options.DefaultModel,
            Provider     = "google",
            Text         = text,
            FinishReason = candidate?.FinishReason ?? "STOP",
            Usage = dto.UsageMetadata is { } u ? new LeapUsage
            {
                PromptTokens     = u.PromptTokenCount,
                CompletionTokens = u.CandidatesTokenCount,
                TotalTokens      = u.TotalTokenCount,
            } : null,
        };
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);

        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden
                => new LeapAuthException($"Google Gemini authentication failed: {body}", "google"),

            System.Net.HttpStatusCode.TooManyRequests
                => new LeapRateLimitException($"Google Gemini rate limit exceeded: {body}", "google"),

            _ => new LeapException(
                $"Google Gemini error ({(int)response.StatusCode}): {body}",
                response.StatusCode, "google")
        };
    }
}
