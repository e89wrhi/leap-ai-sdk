using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Dtos;

internal sealed class OpenAiResponseMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    public List<OpenAiToolCall>? ToolCalls { get; init; }
}
