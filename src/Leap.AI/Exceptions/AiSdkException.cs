using System;
using System.Net;
using AiSdk.Entities.Common;

namespace AiSdk.Exceptions;

public class AiSdkException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public AiError? AiError { get; }
    public string? ProviderName { get; }

    public AiSdkException(string message) : base(message) { }

    public AiSdkException(string message, HttpStatusCode? statusCode, AiError? aiError, string? providerName)
        : base(message)
    {
        StatusCode = statusCode;
        AiError = aiError;
        ProviderName = providerName;
    }

    public AiSdkException(string message, Exception innerException) : base(message, innerException) { }
}
