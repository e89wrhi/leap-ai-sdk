using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Dtos;

internal sealed class AnthropicChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<AnthropicMessage> Messages { get; init; } = [];

    [JsonPropertyName("system")]
    public string? System { get; init; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; init; } = 4096;

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}
