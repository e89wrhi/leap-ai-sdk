using Leap.AI.Core;
using Leap.AI.Providers.Google;

namespace Leap.AI.Core;

/// <summary>Extension methods on <see cref="LeapClientBuilder"/> for the Google Gemini provider.</summary>
public static class GoogleLeapBuilderExtensions
{
    /// <summary>
    /// Configures the client to use Google Gemini with just an API key.
    /// </summary>
    /// <param name="apiKey">Your Google AI Studio API key.</param>
    /// <param name="defaultModel">Model override. Defaults to <c>gemini-1.5-flash</c>.</param>
    public static LeapClientBuilder UseGoogle(
        this LeapClientBuilder builder,
        string apiKey,
        string defaultModel = "gemini-1.5-flash")
        => builder.UseGoogle(new GoogleOptions { ApiKey = apiKey, DefaultModel = defaultModel });

    /// <summary>Configures the client using a fully populated <see cref="GoogleOptions"/> object.</summary>
    public static LeapClientBuilder UseGoogle(
        this LeapClientBuilder builder,
        GoogleOptions options)
        => builder.WithProvider(new GoogleProvider(options));

    /// <summary>Configures the client with a delegate-based options setup.</summary>
    public static LeapClientBuilder UseGoogle(
        this LeapClientBuilder builder,
        Action<GoogleOptions> configure)
    {
        var options = new GoogleOptions { ApiKey = string.Empty };
        configure(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException("Google API key is required.", nameof(configure));

        return builder.WithProvider(new GoogleProvider(options));
    }
}
