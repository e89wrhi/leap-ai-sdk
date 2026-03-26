using AiSdk.Entities.Chat;
using AiSdk.Entities.Common;
using AiSdk.Exceptions;
using AiSdk.Infrastructure;
using AiSdk.Services.Chat;
using System.Text.Json;

namespace AiSdk.Providers.OpenAi;

public class OpenAiModel : ILanguageModel
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "OpenAI";
    public string ModelId { get; }

    public OpenAiModel(string modelId, string apiKey, HttpClient? httpClient = null)
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
            throw new AiSdkException($"OpenAI Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ChatCompletion>(responseBody)!;
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
            throw new AiSdkException($"OpenAI Stream Error: {response.StatusCode}", response.StatusCode, aiError, ProviderName);
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        await foreach (var chunk in SseReader.ReadStreamAsync<ChatCompletionChunk>(stream, cancellationToken))
        {
            yield return chunk;
        }
    }

    private HttpRequestMessage CreateRequestMessage(ChatCreateOptions options)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = ModelId,
            messages = options.Messages,
            stream = options.Stream,
            temperature = options.Temperature
        }), System.Text.Encoding.UTF8, "application/json");

        return request;
    }
}
