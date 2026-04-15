using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;
using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;

namespace Leap.AI.Core.Middleware;

/// <summary>
/// Middleware that retries transient failures with exponential backoff.
/// Respects <c>Retry-After</c> headers on rate-limit responses (HTTP 429).
/// </summary>
public sealed class ResilienceMiddleware : ILeapMiddleware
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public ResilienceMiddleware(int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        _maxRetries = maxRetries;
        _baseDelay  = baseDelay ?? TimeSpan.FromSeconds(1);
    }

    public async Task<ChatResponse> InvokeAsync(
        ChatContext context,
        Func<ChatContext, Task<ChatResponse>> next,
        CancellationToken ct = default)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await next(context);
            }
            catch (LeapRateLimitException ex) when (attempt < _maxRetries)
            {
                attempt++;
                var delay = ex.RetryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, attempt) * _baseDelay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
            catch (LeapException ex) when (IsTransient(ex) && attempt < _maxRetries)
            {
                attempt++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * _baseDelay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }
    }

    private static bool IsTransient(LeapException ex) =>
        ex.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable
                      or System.Net.HttpStatusCode.GatewayTimeout
                      or System.Net.HttpStatusCode.TooManyRequests;
}
