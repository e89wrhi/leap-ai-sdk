using System.Net;
using AiSdk.Entities.Common;

namespace AiSdk.Exceptions;

public class AiRateLimitException : AiSdkException
{
    public AiRateLimitException(string message, HttpStatusCode? statusCode, AiError? aiError, string? providerName)
        : base(message, statusCode, aiError, providerName)
    {
    }
}
