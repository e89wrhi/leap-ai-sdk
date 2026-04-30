using Leap.AI.Core;
using Leap.AI.Providers.Anthropic;

namespace Leap.AI.Core;

/// <summary>
/// Extension methods on <see cref="LeapClientBuilder"/> for the Anthropic Claude provider.
/// </summary>
public static class AnthropicLeapBuilderExtensions
{
    /// <summary>
    /// Configures the LeapClient to use Anthropic as the primary provider.
    /// </summary>
    public static LeapClientBuilder UseAnthropic(
        this LeapClientBuilder builder,
        string apiKey,
        string defaultModel = "claude-3-5-sonnet-latest")
    {
        return builder.UseAnthropic(new AnthropicOptions 
        { 
            ApiKey = apiKey, 
            DefaultModel = defaultModel 
        });
    }

    /// <summary>
    /// Configures the <see cref="LeapClient"/> with a fully-populated <see cref="AnthropicOptions"/> object.
    /// Use this overload when a custom <c>BaseUrl</c>, <c>AnthropicVersion</c>, or <see cref="System.Net.Http.HttpClient"/> is required.
    /// </summary>
    public static LeapClientBuilder UseAnthropic(
        this LeapClientBuilder builder,
        AnthropicOptions options)
    {
        return builder.WithProvider(new AnthropicProvider(options));
    }

    /// <summary>
    /// Configures the <see cref="LeapClient"/> to use Anthropic via a delegate-based options setup.
    /// </summary>
    public static LeapClientBuilder UseAnthropic(
        this LeapClientBuilder builder,
        Action<AnthropicOptions> configure)
    {
        var options = new AnthropicOptions { ApiKey = "" }; 
        configure(options);
        
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            throw new ArgumentException("Anthropic API Key is required. Please set it via the configure action.");
        }

        return builder.WithProvider(new AnthropicProvider(options));
    }
}
