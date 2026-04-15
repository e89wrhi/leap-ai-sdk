using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;

namespace Leap.AI.Core.Abstractions;

/// <summary>
/// Defines a middleware component in the Leap request pipeline.
/// Middlewares are executed in registration order, wrapping the final provider call.
/// Inspired by the ASP.NET Core middleware pattern.
/// </summary>
public interface ILeapMiddleware
{
    /// <summary>
    /// Processes the chat context and calls <paramref name="next"/> to continue the pipeline,
    /// or short-circuits by returning a response directly.
    /// </summary>
    Task<ChatResponse> InvokeAsync(
        ChatContext context,
        Func<ChatContext, Task<ChatResponse>> next,
        CancellationToken ct = default);
}
