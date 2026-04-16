namespace Leap.AI.Providers.Google;

/// <summary>Configuration options for the Google Gemini provider.</summary>
public sealed class GoogleOptions
{
    /// <summary>Google AI Studio API key. Required.</summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Default Gemini model to use when the request does not specify one.
    /// Defaults to <c>gemini-1.5-flash</c> for its speed/quality balance.
    /// </summary>
    public string DefaultModel { get; set; } = "gemini-1.5-flash";

    /// <summary>
    /// Base URL for the Google Generative Language REST API.
    /// Override for custom proxies or Vertex AI endpoints.
    /// </summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>
    /// Optional custom <see cref="HttpClient"/>. When null, a shared instance is created automatically.
    /// </summary>
    public HttpClient? HttpClient { get; set; }
}
