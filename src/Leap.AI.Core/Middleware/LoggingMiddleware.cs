using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;
using Microsoft.Extensions.Logging;

namespace Leap.AI.Core.Middleware;

/// <summary>
/// Middleware that logs each request and response using <see cref="ILogger"/>.
/// Logs provider name, message count, duration, token usage, and finish reason.
/// </summary>
public sealed class LoggingMiddleware : ILeapMiddleware
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger) => _logger = logger;

    // For use without DI
    public LoggingMiddleware(ILogger logger) => _logger = logger;

    public async Task<ChatResponse> InvokeAsync(
        ChatContext context,
        Func<ChatContext, Task<ChatResponse>> next,
        CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "[Leap] → {Provider} | {MessageCount} message(s)",
            context.Provider.Name,
            context.Request.Messages.Count);

        try
        {
            var response = await next(context);
            sw.Stop();

            _logger.LogInformation(
                "[Leap] ← {Provider} | {Duration}ms | finish={FinishReason} | tokens={Total}",
                context.Provider.Name,
                sw.ElapsedMilliseconds,
                response.FinishReason,
                response.Usage?.TotalTokens ?? 0);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "[Leap] ✕ {Provider} failed after {Duration}ms",
                context.Provider.Name,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
