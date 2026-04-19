using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Google.Dtos;

/// <summary>
/// Wrapper envelope used by Gemini's server-sent-events stream.
/// Each SSE data line deserializes to this shape.
/// </summary>
internal sealed class GeminiStreamChunk
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; init; } = [];

    [JsonPropertyName("usageMetadata")]
    public GeminiUsageMetadata? UsageMetadata { get; init; }
}
