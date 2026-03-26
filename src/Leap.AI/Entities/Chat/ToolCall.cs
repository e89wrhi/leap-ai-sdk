using AiSdk.Entities.Base;
using AiSdk.Interfaces;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Chat;

public class ToolCall : AiEntity, IHasId
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}
