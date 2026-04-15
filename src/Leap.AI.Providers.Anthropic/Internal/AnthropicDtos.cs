using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Internal;

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

internal sealed class AnthropicMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

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

internal sealed class AnthropicContent
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "text";

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

internal sealed class AnthropicUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; init; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; init; }
}
