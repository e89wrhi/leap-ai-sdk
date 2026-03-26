using AiSdk.Entities.Chat;
using AiSdk.Entities.Common;
using AiSdk.Exceptions;
using AiSdk.Infrastructure;
using AiSdk.Services.Chat;
using System.Text.Json;

namespace AiSdk.Providers.Google;

public class GoogleModel : ILanguageModel
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Google";
    public string ModelId { get; }

    public GoogleModel(string modelId, string apiKey, HttpClient? httpClient = null)
    {
        ModelId = modelId;
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ChatCompletion> DoGenerateAsync(ChatCreateOptions options, CancellationToken cancellationToken)
    {
        var request = CreateRequestMessage(options, false);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            AiError? aiError = null;
            try { aiError = JsonSerializer.Deserialize<AiErrorResponse>(errorBody)?.Error; } catch { }
            throw new AiSdkException($"Google Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return MapToStandardCompletion(responseBody);
    }

    public async IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(ChatCreateOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options.Stream = true;
        var request = CreateRequestMessage(options, true);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            AiError? aiError = null;
            try { aiError = JsonSerializer.Deserialize<AiErrorResponse>(errorBody)?.Error; } catch { }
            throw new AiSdkException($"Google Stream Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        await foreach (var chunk in SseReader.ReadStreamAsync<ChatCompletionChunk>(stream, cancellationToken))
        {
            yield return chunk;
        }
    }

    private HttpRequestMessage CreateRequestMessage(ChatCreateOptions options, bool isStream)
    {
        string operation = isStream ? "streamGenerateContent" : "generateContent";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:{operation}?key={_apiKey}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        var payload = new
        {
            contents = options.Messages, // In real world we map this to Geminis `contents` schema
            // generationConfig = new { temperature = options.Temperature }
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        return request;
    }

    private ChatCompletion MapToStandardCompletion(string rawJson)
    {
        return JsonSerializer.Deserialize<ChatCompletion>(rawJson)!;
    }
}
