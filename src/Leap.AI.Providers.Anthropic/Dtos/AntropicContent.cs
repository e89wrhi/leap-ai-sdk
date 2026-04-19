using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Dtos;

internal sealed class AnthropicContent
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
