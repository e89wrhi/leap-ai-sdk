namespace Leap.AI.Providers.xAI;

/// <summary>Configuration options for the xAI Grok provider.</summary>
public sealed class XAiOptions
{
    /// <summary>Your xAI API key (required). Obtain from <see href="https://console.x.ai"/>.</summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Base URL for the xAI API. Defaults to <c>https://api.x.ai/v1</c>.
    /// Override for custom proxies or enterprise endpoints.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.x.ai/v1";

    /// <summary>
    /// Default Grok model used when the request does not specify one.
    /// Defaults to <c>grok-3</c>, xAI's most capable frontier model.
    /// <list type="bullet">
    ///   <item><c>grok-3</c> — Most capable, best reasoning.</item>
    ///   <item><c>grok-3-fast</c> — Optimized for speed.</item>
    ///   <item><c>grok-3-mini</c> — Lightweight, cost-efficient.</item>
    ///   <item><c>grok-3-mini-fast</c> — Fastest and smallest.</item>
    ///   <item><c>grok-2</c> — Previous generation.</item>
    /// </list>
    /// </summary>
    public string DefaultModel { get; set; } = "grok-3";

    /// <summary>
    /// Optional custom <see cref="HttpClient"/> to use.
    /// When null, a shared instance is created automatically.
    /// </summary>
    public HttpClient? HttpClient { get; set; }
}
