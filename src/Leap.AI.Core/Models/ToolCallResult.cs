namespace Leap.AI.Core.Models;

/// <summary>
/// Represents a tool call requested by the LLM in a <see cref="ChatResponse"/>.
/// </summary>
public sealed class ToolCallResult
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = "function";
    public required FunctionCallResult Function { get; init; }
}

public sealed class FunctionCallResult
{
    public string Name { get; init; } = string.Empty;
    public string Arguments { get; init; } = string.Empty;
}
