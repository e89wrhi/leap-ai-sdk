using AiSdk.Entities.Chat;
using AiSdk.Services.Chat;

namespace AiSdk.Providers;

public interface ILanguageModel
{
    string ProviderName { get; }
    string ModelId { get; }

    Task<ChatCompletion> DoGenerateAsync(
        ChatCreateOptions options,
        CancellationToken cancellationToken);

    IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(
        ChatCreateOptions options,
        CancellationToken cancellationToken);
}
