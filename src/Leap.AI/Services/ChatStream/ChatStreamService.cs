using AiSdk.Entities.Chat;
using AiSdk.Services.Chat;

namespace AiSdk.Services.ChatStream;

public class ChatStreamService : Service<ChatCompletionChunk>
{
    public ChatStreamService() { }

    public IAsyncEnumerable<ChatCompletionChunk> CreateAsync(ChatCreateOptions options, CancellationToken cancellationToken = default)
    {
        if (options.Model == null)
        {
            throw new System.ArgumentException("Model must be specified.");
        }

        Providers.ILanguageModel model = options.Model.AsT2
            ?? Providers.ModelRegistry.GetModel(options.Model.AsT1!);

        options.Stream = true;
        return model.DoStreamAsync(options, cancellationToken);
    }
}
