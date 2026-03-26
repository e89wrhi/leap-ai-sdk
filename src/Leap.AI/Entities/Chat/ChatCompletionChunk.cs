using AiSdk.Entities.Base;
using AiSdk.Interfaces;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Chat;

public class ChatCompletionChunk : AiEntity, IHasId
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatChunkChoice> Choices { get; set; } = new();
}

public class ChatChunkChoice
{
    [JsonPropertyName("delta")]
    public ChatMessage Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
}
