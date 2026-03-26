using AiSdk.Entities.Base;
using AiSdk.Interfaces;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Chat;

public class ChatCompletion : AiEntity, IHasId
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public Common.AiUsage? Usage { get; set; }
}

public class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = null!;

    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}
