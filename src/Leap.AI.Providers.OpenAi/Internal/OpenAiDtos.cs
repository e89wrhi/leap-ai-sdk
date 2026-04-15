using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Internal;

// ── Outbound ──────────────────────────────────────────────────────────────────

internal sealed class OpenAiChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenAiMessage> Messages { get; init; } = [];

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OpenAiTool>? Tools { get; init; }

    [JsonPropertyName("tool_choice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolChoice { get; init; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ResponseFormat { get; init; }
}

internal sealed class OpenAiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; init; }

    public static OpenAiMessage From(ChatMessage msg) => new()
    {
        Role       = msg.Role,
        Content    = msg.Content,
        ToolCallId = (msg as ToolResultMessage)?.ToolCallId,
    };
}

internal sealed class OpenAiTool
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    [JsonPropertyName("function")]
    public OpenAiFunctionDef Function { get; init; } = null!;
}

internal sealed class OpenAiFunctionDef
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("parameters")]
    public JsonElement Parameters { get; init; }

    [JsonPropertyName("strict")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Strict { get; init; }
}

// ── Inbound ───────────────────────────────────────────────────────────────────

internal sealed class OpenAiChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<OpenAiChoice> Choices { get; init; } = [];

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; init; }
}

internal sealed class OpenAiChoice
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("message")]
    public OpenAiResponseMessage? Message { get; init; }

    [JsonPropertyName("delta")]
    public OpenAiDelta? Delta { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}

internal sealed class OpenAiResponseMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    public List<OpenAiToolCall>? ToolCalls { get; init; }
}

internal sealed class OpenAiDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; init; }
}

internal sealed class OpenAiToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    [JsonPropertyName("function")]
    public OpenAiFunctionCall Function { get; init; } = null!;
}

internal sealed class OpenAiFunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("arguments")]
    public string Arguments { get; init; } = string.Empty;
}

internal sealed class OpenAiUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}
