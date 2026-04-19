using Leap.AI.Core.Middleware;
using Microsoft.Extensions.Logging;

namespace Leap.AI.Core;

/// <summary>
/// Extension methods on <see cref="LeapClientBuilder"/> for built-in middleware.
/// </summary>
public static class LeapBuilderMiddlewareExtensions
{
    /// <summary>
    /// Adds structured request/response logging to the pipeline.
    /// </summary>
    public static LeapClientBuilder UseLogging(
        this LeapClientBuilder builder,
        ILogger? logger = null)
    {
        var log = logger ?? LoggerFactory
            .Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information))
            .CreateLogger<LoggingMiddleware>();

        return builder.UseMiddleware(new LoggingMiddleware(log));
    }

    /// <summary>
    /// Adds automatic retry with exponential back-off for transient and rate-limit errors.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts (default: 3).</param>
    /// <param name="baseDelay">Base delay for exponential backoff (default: 1 second).</param>
    public static LeapClientBuilder UseRetry(
        this LeapClientBuilder builder,
        int maxRetries = 3,
        TimeSpan? baseDelay = null)
        => builder.UseMiddleware(new ResilienceMiddleware(maxRetries, baseDelay));
}
