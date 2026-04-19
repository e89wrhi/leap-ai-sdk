namespace Leap.AI.Core.Models;

/// <summary>
/// Token usage information for a completed AI request.
/// </summary>
public sealed class LeapUsage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
}
