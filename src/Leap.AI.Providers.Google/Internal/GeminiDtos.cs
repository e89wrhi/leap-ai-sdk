using System.Text.Json.Serialization;

namespace Leap.AI.Providers.Google.Internal;

// ── Outbound ──────────────────────────────────────────────────────────────────

/// <summary>Top-level request body for the Gemini generateContent endpoint.</summary>
internal sealed class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; init; } = [];

    [JsonPropertyName("systemInstruction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiContent? SystemInstruction { get; init; }

    [JsonPropertyName("generationConfig")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeminiGenerationConfig? GenerationConfig { get; init; }
}

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

internal sealed class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; init; }

    [JsonPropertyName("maxOutputTokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxOutputTokens { get; init; }

    [JsonPropertyName("responseMimeType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseMimeType { get; init; }
}

// ── Inbound ───────────────────────────────────────────────────────────────────

/// <summary>Top-level response from the Gemini generateContent endpoint.</summary>
internal sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; init; } = [];

    [JsonPropertyName("usageMetadata")]
    public GeminiUsageMetadata? UsageMetadata { get; init; }
}

internal sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; init; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; init; }

    [JsonPropertyName("index")]
    public int Index { get; init; }
}

internal sealed class GeminiUsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; init; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; init; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; init; }
}

// ── Streaming ─────────────────────────────────────────────────────────────────

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
