namespace Leap.AI.Providers.OpenAi;

/// <summary>
/// Configuration options for the OpenAI provider.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>Your OpenAI API key (required).</summary>
    public required string ApiKey { get; set; }

    /// <summary>Base URL of the OpenAI API. Override for Azure OpenAI or local proxies.</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Default model used when the request does not specify one.</summary>
    public string DefaultModel { get; set; } = "gpt-4o";

    /// <summary>OpenAI Organization ID (optional).</summary>
    public string? OrganizationId { get; set; }

    /// <summary>OpenAI Project ID (optional).</summary>
    public string? ProjectId { get; set; }

    /// <summary>Custom <see cref="HttpClient"/> to use. When null, a shared instance is created.</summary>
    public HttpClient? HttpClient { get; set; }
}
