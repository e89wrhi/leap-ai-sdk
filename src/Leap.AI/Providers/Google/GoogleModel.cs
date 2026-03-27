using AiSdk.Entities.Chat;
using AiSdk.Services.Chat;
using AiSdk.Infrastructure;
using System.Text.Json;

namespace AiSdk.Providers.Google;

public class GoogleModel : BaseLanguageModel
{
    public override string ProviderName => "Google";

    public GoogleModel(string modelId, string? apiKey = null, HttpClient? httpClient = null)
        : base(modelId, apiKey, httpClient)
    {
    }

    public override async Task<ChatCompletion> DoGenerateAsync(ChatCreateOptions options, CancellationToken cancellationToken)
    {
        var response = await SendWithRetryAsync(() => CreateRequestMessage(options, false), cancellationToken);

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
        var response = await SendWithRetryAsync(() => CreateRequestMessage(options, true), cancellationToken);

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

    private HttpRequestMessage CreateRequestMessage(ChatCreateOptions options, bool isStream)
    {
        string operation = isStream ? "streamGenerateContent" : "generateContent";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:{operation}?key={_apiKey}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var payload = new
        {
            contents = options.Messages,
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        return request;
    }

    private ChatCompletion MapToStandardCompletion(string rawJson)
    {
        return JsonSerializer.Deserialize<ChatCompletion>(rawJson)!;
    }
}
