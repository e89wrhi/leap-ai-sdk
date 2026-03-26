using AiSdk.Entities.Common;
using System.Net;

namespace AiSdk.Exceptions;

public class AiAuthenticationException : AiSdkException
{
    public AiAuthenticationException(string message, HttpStatusCode? statusCode, AiError? aiError, string? providerName)
        : base(message, statusCode, aiError, providerName)
    {
    }
}
