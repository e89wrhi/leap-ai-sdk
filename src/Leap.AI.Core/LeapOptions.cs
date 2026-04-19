using System.Text.Json;

namespace Leap.AI.Core;

/// <summary>
/// Global configuration options for a <see cref="LeapClient"/> instance.
/// Configure via <see cref="LeapClientBuilder.WithOptions"/>.
/// </summary>
public sealed class LeapOptions
{
    /// <summary>Default temperature applied to all requests (0.0–2.0). Null = provider default.</summary>
    public double? DefaultTemperature { get; set; }

    /// <summary>Default max tokens applied to all requests. Null = provider default.</summary>
    public int? DefaultMaxTokens { get; set; }

    /// <summary>System prompt automatically prepended to every conversation.</summary>
    public string? DefaultSystemPrompt { get; set; }

    /// <summary>Maximum iterations the client will follow tool-call chains before stopping.</summary>
    public int MaxToolIterations { get; set; } = 10;

    /// <summary>Maximum retry attempts when structured output fails JSON validation.</summary>
    public int MaxStructuredOutputRetries { get; set; } = 3;

    /// <summary>JSON deserialization options used by <c>GenerateObjectAsync&lt;T&gt;</c>.</summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };
}
