using AiSdk.Entities.Base;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Chat;

public class ChatMessage : AiEntity
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}
