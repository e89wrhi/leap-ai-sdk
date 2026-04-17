using System.Net;

namespace Leap.AI.Core.Errors;

/// <summary>Thrown when the provider rejects the request due to invalid credentials.</summary>
public sealed class LeapAuthException : LeapException
{
    public LeapAuthException(string message, string? providerName = null)
        : base(message, HttpStatusCode.Unauthorized, providerName) { }
}
