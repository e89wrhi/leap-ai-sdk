using AiSdk.Entities.Chat;
using AiSdk.Entities.Common;
using AiSdk.Exceptions;
using AiSdk.Infrastructure;
using AiSdk.Services.Chat;
using System.Text.Json;

namespace AiSdk.Providers.Anthropic;

public class AnthropicModel : ILanguageModel
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Anthropic";
    public string ModelId { get; }

    public AnthropicModel(string modelId, string apiKey, HttpClient? httpClient = null)
    {
        ModelId = modelId;
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ChatCompletion> DoGenerateAsync(ChatCreateOptions options, CancellationToken cancellationToken)
    {
        var request = CreateRequestMessage(options);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            AiError? aiError = null;
            try { aiError = JsonSerializer.Deserialize<AiErrorResponse>(errorBody)?.Error; } catch { }
            throw new AiSdkException($"Anthropic Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        // In a real adapter, this parses Anthropic's unique schema into standard ChatCompletion
        return MapToStandardCompletion(responseBody);
    }

    public async IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(ChatCreateOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options.Stream = true;
        var request = CreateRequestMessage(options);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            AiError? aiError = null;
            try { aiError = JsonSerializer.Deserialize<AiErrorResponse>(errorBody)?.Error; } catch { }
            throw new AiSdkException($"Anthropic Stream Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        await foreach (var chunk in SseReader.ReadStreamAsync<ChatCompletionChunk>(stream, cancellationToken))
        {
            // In a real adapter, map from Anthropic's stream to `ChatCompletionChunk`
            yield return chunk;
        }
    }

    private HttpRequestMessage CreateRequestMessage(ChatCreateOptions options)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        // Basic mapping logic matching Stripe pattern expectation
        var payload = new
        {
            model = ModelId,
            messages = options.Messages,
            stream = options.Stream,
            temperature = options.Temperature
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        return request;
    }

    private ChatCompletion MapToStandardCompletion(string rawJson)
    {
        // Placeholder: dummy parsing to return Standard Entity
        return JsonSerializer.Deserialize<ChatCompletion>(rawJson)!;
    }
}
