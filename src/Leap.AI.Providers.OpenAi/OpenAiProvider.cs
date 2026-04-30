using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;
using Leap.AI.Core.Models;
using Leap.AI.Providers.OpenAi.Dtos;

namespace Leap.AI.Providers.OpenAi;

/// <summary>
/// Leap AI provider for OpenAI Chat Completions API.
/// Supports GPT-4o, GPT-4, GPT-3.5-turbo with full streaming and tool-calling.
/// </summary>
public sealed class OpenAiProvider : ILeapProvider
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly OpenAiOptions _opts;
    private readonly HttpClient _http;

    public string Name => "openai";

    public OpenAiProvider(OpenAiOptions options)
    {
        _opts = options;
        _http = options.HttpClient ?? new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.ApiKey);

        if (options.OrganizationId is not null)
            _http.DefaultRequestHeaders.TryAddWithoutValidation("OpenAI-Organization", options.OrganizationId);
        if (options.ProjectId is not null)
            _http.DefaultRequestHeaders.TryAddWithoutValidation("OpenAI-Project", options.ProjectId);
    }

    public async Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken ct = default)
    {
        var body = Serialize(BuildBody(request, stream: false));
        using var response = await PostAsync(body, ct);
        await EnsureSuccessAsync(response, ct);

        var json = await response.Content.ReadAsStringAsync(ct);
        var dto  = JsonSerializer.Deserialize<OpenAiChatResponse>(json)!;
        return Map(dto);
    }

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var body = Serialize(BuildBody(request, stream: true));
        using var response = await PostAsync(body, ct, responseHeadersRead: true);
        await EnsureSuccessAsync(response, ct);

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            OpenAiChatResponse? chunk;
            try   { chunk = JsonSerializer.Deserialize<OpenAiChatResponse>(data); }
            catch { continue; }
            if (chunk is null) continue;

            var choice = chunk.Choices.FirstOrDefault();
            if (choice is null) continue;

            var text       = choice.Delta?.Content ?? string.Empty;
            var isComplete = choice.FinishReason is not null;

            yield return new ChatChunk
            {
                Text         = text,
                IsComplete   = isComplete,
                FinishReason = choice.FinishReason,
                Usage = chunk.Usage is { } u ? new LeapUsage
                {
                    PromptTokens     = u.PromptTokens,
                    CompletionTokens = u.CompletionTokens,
                    TotalTokens      = u.TotalTokens,
                } : null,
            };
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private OpenAiChatRequest BuildBody(ChatRequest req, bool stream) => new()
    {
        Model       = _opts.DefaultModel,
        Messages    = req.Messages.Select(OpenAiMessage.From).ToList(),
        Temperature = req.Temperature,
        MaxTokens   = req.MaxTokens,
        Stream      = stream,
        StreamOptions = stream ? new { include_usage = true } : null,
        Tools       = req.Tools?.Select(t => new OpenAiTool
        {
            Function = new OpenAiFunctionDef
            {
                Name        = t.Function.Name,
                Description = t.Function.Description,
                Parameters  = t.Function.Parameters,
                Strict      = t.Function.Strict,
            }
        }).ToList(),
        ToolChoice     = req.ToolChoice,
        ResponseFormat = req.ResponseFormat,
    };

    private StringContent Serialize(object body)
    {
        var json = JsonSerializer.Serialize(body, _json);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private Task<HttpResponseMessage> PostAsync(
        StringContent body, CancellationToken ct, bool responseHeadersRead = false)
    {
        var url = $"{_opts.BaseUrl.TrimEnd('/')}/chat/completions";
        if (responseHeadersRead)
            return _http.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, url) { Content = body },
                HttpCompletionOption.ResponseHeadersRead, ct);
        return _http.PostAsync(url, body, ct);
    }

    private static ChatResponse Map(OpenAiChatResponse dto)
    {
        var choice = dto.Choices.FirstOrDefault();
        return new ChatResponse
        {
            Id           = dto.Id,
            Model        = dto.Model,
            Provider     = "openai",
            Text         = choice?.Message?.Content ?? string.Empty,
            FinishReason = choice?.FinishReason ?? string.Empty,
            ToolCalls    = choice?.Message?.ToolCalls?.Select(tc => new ToolCallResult
            {
                Id   = tc.Id,
                Type = tc.Type,
                Function = new FunctionCallResult
                {
                    Name      = tc.Function.Name,
                    Arguments = tc.Function.Arguments,
                },
            }).ToList(),
            Usage = dto.Usage is { } u ? new LeapUsage
            {
                PromptTokens     = u.PromptTokens,
                CompletionTokens = u.CompletionTokens,
                TotalTokens      = u.TotalTokens,
            } : null,
        };
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync(ct);

        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized or
            System.Net.HttpStatusCode.Forbidden =>
                new LeapAuthException($"OpenAI authentication failed: {body}", "openai"),

            System.Net.HttpStatusCode.TooManyRequests =>
                new LeapRateLimitException($"OpenAI rate limit exceeded: {body}", "openai",
                    ParseRetryAfter(response)),

            _ => new LeapException(
                $"OpenAI request failed ({(int)response.StatusCode}): {body}",
                response.StatusCode, "openai"),
        };
    }

    private static TimeSpan? ParseRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta) return delta;
        if (response.Headers.RetryAfter?.Date is { } date)  return date - DateTimeOffset.UtcNow;
        return null;
    }
}
