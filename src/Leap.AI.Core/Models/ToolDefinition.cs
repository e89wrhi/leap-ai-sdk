using System.Text.Json;

namespace Leap.AI.Core.Models;

/// <summary>
/// Describes a tool available to the LLM during a conversation.
/// </summary>
public sealed class ToolDefinition
{
    public string Type { get; init; } = "function";
    public required FunctionDefinition Function { get; init; }
}

public sealed class FunctionDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required JsonElement Parameters { get; init; }
    public bool Strict { get; init; } = false;
}
