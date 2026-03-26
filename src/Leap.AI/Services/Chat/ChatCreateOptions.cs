using AiSdk.Entities.Chat;
using AiSdk.Infrastructure;
using AiSdk.Providers;

namespace AiSdk.Services.Chat;

public class ChatCreateOptions
{
    public AiAnyOf<string, ILanguageModel> Model { get; set; }

    public List<ChatMessage> Messages { get; set; }

    public double? Temperature { get; set; }

    public bool Stream { get; set; }

    public object? ResponseFormat { get; set; }

    public List<Attachment>? Attachments { get; set; }
}
