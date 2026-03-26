using AiSdk.Entities.Chat;
using AiSdk.Providers;

namespace AiSdk.Services.Chat;

public class ChatService : Service<ChatCompletion>
{
    public ChatService() { }

    public Task<ChatCompletion> CreateAsync(ChatCreateOptions options, CancellationToken cancellationToken = default)
    {
        if (options.Model == null)
        {
            throw new ArgumentException("Model must be specified.");
        }

        ILanguageModel model = options.Model.AsT2
            ?? Providers.ModelRegistry.GetModel(options.Model.AsT1!);

        options.Stream = false;
        return model.DoGenerateAsync(options, cancellationToken);
    }
}
