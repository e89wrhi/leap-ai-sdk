using Leap.AI.Core;
using Leap.AI.Providers.xAI;

namespace Leap.AI.Core;

/// <summary>
/// Extension methods on <see cref="LeapClientBuilder"/> for the xAI Grok provider.
/// </summary>
public static class XAiLeapBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="LeapClient"/> to use xAI Grok with just an API key.
    /// </summary>
    /// <param name="apiKey">Your xAI API key from <see href="https://console.x.ai"/>.</param>
    /// <param name="defaultModel">
    /// Model override. Defaults to <c>grok-3</c>.
    /// Other options: <c>grok-3-fast</c>, <c>grok-3-mini</c>, <c>grok-3-mini-fast</c>, <c>grok-2</c>.
    /// </param>
    public static LeapClientBuilder UseXAi(
        this LeapClientBuilder builder,
        string apiKey,
        string defaultModel = "grok-3")
        => builder.UseXAi(new XAiOptions { ApiKey = apiKey, DefaultModel = defaultModel });

    /// <summary>
    /// Configures the <see cref="LeapClient"/> with a fully-populated <see cref="XAiOptions"/> object.
    /// Use this overload when a custom <c>BaseUrl</c> or <see cref="System.Net.Http.HttpClient"/> is required.
    /// </summary>
    public static LeapClientBuilder UseXAi(
        this LeapClientBuilder builder,
        XAiOptions options)
        => builder.WithProvider(new XAiProvider(options));

    /// <summary>
    /// Configures the <see cref="LeapClient"/> to use xAI via a delegate-based options setup.
    /// </summary>
    /// <example>
    /// <code>
    /// LeapClient.Create()
    ///     .UseXAi(o =>
    ///     {
    ///         o.ApiKey       = "xai-...";
    ///         o.DefaultModel = "grok-3-fast";
    ///     })
    ///     .Build();
    /// </code>
    /// </example>
    public static LeapClientBuilder UseXAi(
        this LeapClientBuilder builder,
        Action<XAiOptions> configure)
    {
        var options = new XAiOptions { ApiKey = string.Empty };
        configure(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("xAI API key is required.", nameof(configure));

        return builder.UseXAi(options);
    }
}
