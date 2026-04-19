namespace Leap.AI.Core.Models;

/// <summary>
/// Represents a complete, non-streaming response from an AI provider.
/// </summary>
public sealed class ChatResponse
{
    public string Id { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;

    /// <summary>The primary text content of the response.</summary>
    public string Text { get; init; } = string.Empty;

    public string FinishReason { get; init; } = string.Empty;
    public List<ToolCallResult>? ToolCalls { get; init; }
    public LeapUsage? Usage { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
