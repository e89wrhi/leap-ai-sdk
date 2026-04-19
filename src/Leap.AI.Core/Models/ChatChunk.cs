namespace Leap.AI.Core.Models;

/// <summary>
/// A single streaming chunk from an AI provider.
/// Consume via <c>await foreach</c> on <see cref="IAsyncEnumerable{ChatChunk}"/>.
/// </summary>
public sealed class ChatChunk
{
    /// <summary>The incremental text content of this chunk.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>True when this is the final chunk in the stream.</summary>
    public bool IsComplete { get; init; }

    public string? FinishReason { get; init; }

    /// <summary>Token usage, only populated on the final chunk (if provider supports it).</summary>
    public LeapUsage? Usage { get; init; }
}
