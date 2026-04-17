using System.Net;

namespace Leap.AI.Core.Errors;

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
