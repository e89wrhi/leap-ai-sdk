using Leap.AI.Core;
using Leap.AI.Providers.Anthropic;

namespace Leap.AI.Core;

/// <summary>
/// I've created these extensions to make it effortless to plug Anthropic into your AI pipeline.
/// Simply call .UseAnthropic("your-key") and you're good to go.
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
    /// Configures the LeapClient with full Anthropic options.
    /// This is where I recommend going if you need custom base URLs for proxies or specific API versions.
    /// </summary>
    public static LeapClientBuilder UseAnthropic(
        this LeapClientBuilder builder,
        AnthropicOptions options)
    {
        return builder.WithProvider(new AnthropicProvider(options));
    }

    /// <summary>
    /// If you prefer a more functional configuration style, I've got you covered here.
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
