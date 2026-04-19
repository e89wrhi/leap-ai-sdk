using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;

namespace Leap.AI.Core.Pipeline;

/// <summary>
/// Context object passed through the middleware pipeline for each request.
/// Middlewares can read/write the Request, inspect the Provider, and share
/// arbitrary data via the <see cref="Items"/> bag.
/// </summary>
public sealed class ChatContext
{
    public required ChatRequest Request { get; set; }
    public ChatResponse? Response { get; set; }
    public required ILeapProvider Provider { get; set; }

    /// <summary>Shared key-value store for middleware communication.</summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
}
