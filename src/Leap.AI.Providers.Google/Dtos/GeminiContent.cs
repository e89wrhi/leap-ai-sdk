using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Google.Dtos;

internal sealed class GeminiContent
{
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Role { get; init; }

    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; init; } = [];
}

internal sealed class GeminiPart
{
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Text { get; init; }
}

