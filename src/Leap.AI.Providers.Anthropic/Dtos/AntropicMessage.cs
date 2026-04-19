using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Dtos;

internal sealed class AnthropicMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}
