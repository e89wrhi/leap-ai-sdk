using AiSdk.Entities.Chat;
using AiSdk.Services.Chat;
using AiSdk.Infrastructure;
using System.Text.Json;

namespace AiSdk.Providers.OpenAi;

public class OpenAiModel : BaseLanguageModel
{
    public override string ProviderName => "OpenAI";

    public OpenAiModel(string modelId, string? apiKey = null, HttpClient? httpClient = null)
        : base(modelId, apiKey, httpClient)
    {
    }

    public override async Task<ChatCompletion> DoGenerateAsync(ChatCreateOptions options, CancellationToken cancellationToken)
    {
        var response = await SendWithRetryAsync(() => CreateRequestMessage(options), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, ProviderName);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ChatCompletion>(responseBody)!;
    }

    public override async IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(ChatCreateOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options.Stream = true;
        // Streaming doesn't easily support automatic retries for the stream itself once it starts,
        // but we can retry the initial connection.
        var response = await SendWithRetryAsync(() => CreateRequestMessage(options), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, $"{ProviderName} Stream");
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
