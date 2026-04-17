using System.Net;

namespace Leap.AI.Core.Errors;

/// <summary>Thrown when structured output deserialization fails after all retries.</summary>
public sealed class LeapValidationException : LeapException
{
    public string? Field { get; }

    public LeapValidationException(string message, string? field = null, Exception? inner = null)
        : base(message, inner ?? new Exception(message))
    {
        Field = field;
    }
}
