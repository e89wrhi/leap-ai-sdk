using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Dtos;

internal sealed class AnthropicChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public List<AnthropicContent> Content { get; init; } = [];

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; init; }

    [JsonPropertyName("usage")]
    public AnthropicUsage? Usage { get; init; }
}
