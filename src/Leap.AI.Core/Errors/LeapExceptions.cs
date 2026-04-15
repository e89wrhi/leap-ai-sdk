using System.Net;

namespace Leap.AI.Core.Errors;

/// <summary>Thrown when the provider rejects the request due to invalid credentials.</summary>
public sealed class LeapAuthException : LeapException
{
    public LeapAuthException(string message, string? providerName = null)
        : base(message, HttpStatusCode.Unauthorized, providerName) { }
}

/// <summary>Thrown when the provider rate-limits the request (HTTP 429).</summary>
public sealed class LeapRateLimitException : LeapException
{
    public TimeSpan? RetryAfter { get; }

    public LeapRateLimitException(string message, string? providerName = null, TimeSpan? retryAfter = null)
        : base(message, HttpStatusCode.TooManyRequests, providerName)
    {
        RetryAfter = retryAfter;
    }
}

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
