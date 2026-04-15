using System.Net;

namespace Leap.AI.Core.Errors;

/// <summary>
/// Base exception for all Leap AI SDK errors.
/// </summary>
public class LeapException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? ProviderName { get; }
    public string? ErrorCode { get; }

    public LeapException(string message) : base(message) { }

    public LeapException(string message, Exception innerException)
        : base(message, innerException) { }

    public LeapException(string message, HttpStatusCode statusCode,
        string? providerName = null, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
        ProviderName = providerName;
        ErrorCode = errorCode;
    }
}
