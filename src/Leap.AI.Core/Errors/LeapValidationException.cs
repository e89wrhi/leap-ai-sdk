namespace Leap.AI.Core.Errors;

/// <summary>
/// Thrown when structured output (<c>GenerateObjectAsync&lt;T&gt;</c>) fails to produce
/// valid JSON after exhausting all configured retry attempts.
/// </summary>
public sealed class LeapValidationException : LeapException
{
    /// <summary>
    /// The field or schema path that caused the validation failure, if known.
    /// </summary>
    public string? Field { get; }

    /// <param name="message">Human-readable description of the failure.</param>
    /// <param name="field">Optional field or schema path that triggered the error.</param>
    /// <param name="inner">The underlying <see cref="System.Text.Json.JsonException"/>, if any.</param>
    public LeapValidationException(string message, string? field = null, Exception? inner = null)
        : base(message, inner)
    {
        Field = field;
    }
}
