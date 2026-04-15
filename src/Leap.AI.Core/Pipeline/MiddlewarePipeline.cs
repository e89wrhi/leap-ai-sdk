using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;

namespace Leap.AI.Core.Pipeline;

/// <summary>
/// Executes an ordered chain of <see cref="ILeapMiddleware"/> instances before
/// invoking the terminal <see cref="ILeapProvider"/>.
/// Modeled after the ASP.NET Core request pipeline.
/// </summary>
public sealed class MiddlewarePipeline
{
    private readonly IReadOnlyList<ILeapMiddleware> _middlewares;
    private readonly ILeapProvider _provider;

    public MiddlewarePipeline(IReadOnlyList<ILeapMiddleware> middlewares, ILeapProvider provider)
    {
        _middlewares = middlewares;
        _provider = provider;
    }

    public Task<ChatResponse> ExecuteAsync(ChatRequest request, CancellationToken ct = default)
    {
        var context = new ChatContext { Request = request, Provider = _provider };
        return ExecuteAt(context, 0, ct);
    }

    private Task<ChatResponse> ExecuteAt(ChatContext context, int index, CancellationToken ct)
    {
        // Terminal: invoke the provider directly
        if (index >= _middlewares.Count)
            return context.Provider.GenerateAsync(context.Request, ct);

        // Otherwise, call next middleware, passing a delegate to advance the chain
        return _middlewares[index].InvokeAsync(
            context,
            ctx => ExecuteAt(ctx, index + 1, ct),
            ct);
    }
}
