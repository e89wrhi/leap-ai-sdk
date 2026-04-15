using System.Text.Json;

namespace Leap.AI.Core.Abstractions;

/// <summary>
/// Defines a tool (function) that can be registered with <see cref="LeapClient"/>
/// and automatically invoked by the LLM during a multi-turn conversation.
/// </summary>
public interface ILeapTool
{
    /// <summary>The unique name used by the LLM to call this tool.</summary>
    string Name { get; }

    /// <summary>Human-readable description of what this tool does. Sent to the LLM.</summary>
    string Description { get; }

    /// <summary>JSON Schema describing the tool's input parameters.</summary>
    JsonElement ParametersSchema { get; }

    /// <summary>
    /// Executes the tool with the given JSON <paramref name="arguments"/> string
    /// and returns a string result to feed back to the LLM.
    /// </summary>
    Task<string> ExecuteAsync(string arguments, CancellationToken ct = default);
}
