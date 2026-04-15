using Leap.AI.Core;
using Leap.AI.Providers.OpenAi;

namespace Leap.AI.Core;

/// <summary>
/// Extension methods for LeapClientBuilder to enable OpenAI provider.
/// </summary>
public static class OpenAiLeapBuilderExtensions
{
    /// <summary>
    /// Configures the LeapClient to use OpenAI as the primary provider.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="apiKey">OpenAI API Key.</param>
    /// <param name="defaultModel">The default model to use (default: gpt-4o).</param>
    public static LeapClientBuilder UseOpenAi(
        this LeapClientBuilder builder,
        string apiKey,
        string defaultModel = "gpt-4o")
    {
        return builder.UseOpenAi(new OpenAiOptions 
        { 
            ApiKey = apiKey, 
            DefaultModel = defaultModel 
        });
    }

    /// <summary>
    /// Configures the LeapClient with full OpenAI options.
    /// </summary>
    public static LeapClientBuilder UseOpenAi(
        this LeapClientBuilder builder,
        OpenAiOptions options)
    {
        return builder.WithProvider(new OpenAiProvider(options));
    }

    /// <summary>
    /// Configures the LeapClient with OpenAI options using a config action.
    /// </summary>
    public static LeapClientBuilder UseOpenAi(
        this LeapClientBuilder builder,
        Action<OpenAiOptions> configure)
    {
        var options = new OpenAiOptions { ApiKey = string.Empty }; // Placeholder, will be set in configure
        configure(options);
        
        if (string.IsNullOrEmpty(options.ApiKey))
        {
            throw new ArgumentException("OpenAI API Key is required.");
        }

        return builder.WithProvider(new OpenAiProvider(options));
    }
}
