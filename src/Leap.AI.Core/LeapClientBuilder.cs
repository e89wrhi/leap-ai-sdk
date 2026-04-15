using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Tools;

namespace Leap.AI.Core;

/// <summary>
/// Fluent builder for constructing a <see cref="LeapClient"/>.
/// <para>
/// <code>
/// var leap = LeapClient.Create()
///     .UseOpenAi("sk-...")
///     .UseLogging()
///     .UseRetry(3)
///     .Build();
/// </code>
/// </para>
/// </summary>
public sealed class LeapClientBuilder
{
    private ILeapProvider? _provider;
    private readonly List<ILeapMiddleware> _middlewares = [];
    private readonly ToolRegistry _toolRegistry = new();
    private readonly LeapOptions _options = new();

    internal LeapClientBuilder() { }

    // ── Provider ─────────────────────────────────────────────────────────────

    /// <summary>Sets the AI provider. Called by provider extension methods (e.g., .UseOpenAi()).</summary>
    public LeapClientBuilder WithProvider(ILeapProvider provider)
    {
        _provider = provider;
        return this;
    }

    // ── Middleware ────────────────────────────────────────────────────────────

    /// <summary>Adds a middleware to the pipeline (executed in registration order).</summary>
    public LeapClientBuilder UseMiddleware(ILeapMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>Adds a middleware from a delegate — useful for simple inline middleware.</summary>
    public LeapClientBuilder UseMiddleware(
        Func<ChatContext, Func<ChatContext, Task<ChatResponse>>, CancellationToken, Task<ChatResponse>> handler)
    {
        _middlewares.Add(new DelegateMiddleware(handler));
        return this;
    }

    // ── Tools ─────────────────────────────────────────────────────────────────

    /// <summary>Registers a tool the LLM can invoke during conversations.</summary>
    public LeapClientBuilder UseTool(ILeapTool tool)
    {
        _toolRegistry.Register(tool);
        return this;
    }

    // ── Options ───────────────────────────────────────────────────────────────

    /// <summary>Configures global default options (temperature, system prompt, retries, etc.).</summary>
    public LeapClientBuilder WithOptions(Action<LeapOptions> configure)
    {
        configure(_options);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    /// <summary>Builds and returns the configured <see cref="LeapClient"/>.</summary>
    public LeapClient Build()
    {
        if (_provider is null)
            throw new InvalidOperationException(
                "No provider configured. Call a provider extension method such as .UseOpenAi(\"key\") " +
                "or .WithProvider(myProvider) before calling .Build().");

        return new LeapClient(_provider, [.. _middlewares], _toolRegistry, _options);
    }
}

// ── Internal Helpers ──────────────────────────────────────────────────────────

using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;

internal sealed class DelegateMiddleware(
    Func<ChatContext, Func<ChatContext, Task<ChatResponse>>, CancellationToken, Task<ChatResponse>> _handler)
    : ILeapMiddleware
{
    public Task<ChatResponse> InvokeAsync(
        ChatContext context,
        Func<ChatContext, Task<ChatResponse>> next,
        CancellationToken ct = default)
        => _handler(context, next, ct);
}
