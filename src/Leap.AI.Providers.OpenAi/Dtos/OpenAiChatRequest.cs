using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Dtos;

internal sealed class OpenAiChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenAiMessage> Messages { get; init; } = [];

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OpenAiTool>? Tools { get; init; }

    [JsonPropertyName("tool_choice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolChoice { get; init; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ResponseFormat { get; init; }

    [JsonPropertyName("stream_options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? StreamOptions { get; init; }
}
