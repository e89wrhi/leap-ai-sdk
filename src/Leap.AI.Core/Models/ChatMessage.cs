namespace Leap.AI.Core.Models;

/// <summary>
/// Represents a single message in a chat conversation.
/// Use the static factory methods to create role-specific messages.
/// </summary>
public class ChatMessage
{
    public string Role { get; }
    public string Content { get; }

    protected ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }

    /// <summary>Creates a system instruction message.</summary>
    public static ChatMessage System(string content) => new("system", content);

    /// <summary>Creates a user message.</summary>
    public static ChatMessage User(string content) => new("user", content);

    /// <summary>Creates an assistant response message.</summary>
    public static ChatMessage Assistant(string content) => new("assistant", content);

    /// <summary>Creates a tool result message to send back after a tool call.</summary>
    public static ToolResultMessage ToolResult(string content, string toolCallId)
        => new("tool", content, toolCallId);
}

/// <summary>
/// A chat message that carries the result of an LLM tool invocation.
/// </summary>
public sealed class ToolResultMessage : ChatMessage
{
    public string ToolCallId { get; }

    public ToolResultMessage(string role, string content, string toolCallId)
        : base(role, content)
    {
        ToolCallId = toolCallId;
    }
}
