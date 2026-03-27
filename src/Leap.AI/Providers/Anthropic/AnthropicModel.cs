using AiSdk.Entities.Chat;
using AiSdk.Services.Chat;
using AiSdk.Infrastructure;
using System.Text.Json;

namespace AiSdk.Providers.Anthropic;

public class AnthropicModel : BaseLanguageModel
{
    public override string ProviderName => "Anthropic";

    public AnthropicModel(string modelId, string? apiKey = null, HttpClient? httpClient = null)
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
        return MapToStandardCompletion(responseBody);
    }

    public override async IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(ChatCreateOptions options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        options.Stream = true;
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
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

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
        return JsonSerializer.Deserialize<ChatCompletion>(rawJson)!;
    }
}
