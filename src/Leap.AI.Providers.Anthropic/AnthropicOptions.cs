namespace Leap.AI.Providers.Anthropic;

/// <summary>Configuration options for the Anthropic Claude provider.</summary>
public sealed class AnthropicOptions
{
    /// <summary>Your Anthropic API key (required).</summary>
    public required string ApiKey { get; set; }

    /// <summary>Base URL for the Anthropic Messages API. Override for proxies or custom endpoints.</summary>
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";

    /// <summary>Default Claude model used when the request does not specify one.</summary>
    public string DefaultModel { get; set; } = "claude-3-5-sonnet-latest";

    /// <summary>
    /// Anthropic API version header value. Pinned to <c>2023-06-01</c> by default.
    /// Only change this if Anthropic releases a new stable API version.
    /// </summary>
    public string AnthropicVersion { get; set; } = "2023-06-01";

    /// <summary>Custom <see cref="HttpClient"/> to use. When null, a shared instance is created automatically.</summary>
    public HttpClient? HttpClient { get; set; }
}
