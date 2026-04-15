using System.Text.Json;

namespace Leap.AI.Core.Models;

/// <summary>
/// Represents a request to an AI provider for chat completion.
/// </summary>
public sealed class ChatRequest
{
    public List<ChatMessage> Messages { get; set; } = [];
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public List<ToolDefinition>? Tools { get; set; }
    public string? ToolChoice { get; set; }
    public bool Stream { get; set; }
    public object? ResponseFormat { get; set; }
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}
