using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Anthropic.Dtos;

internal sealed class AnthropicUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; init; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; init; }
}
