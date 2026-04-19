using Leap.AI.Core.Models;

namespace Leap.AI.Core.Abstractions;

/// <summary>
/// Core abstraction for an AI language model provider.
/// Implement this interface to add support for any LLM service.
/// </summary>
public interface ILeapProvider
{
    /// <summary>Unique identifier for this provider (e.g., "openai", "anthropic", "google").</summary>
    string Name { get; }

    /// <summary>Generates a complete chat response for the given request.</summary>
    Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken ct = default);

    /// <summary>Generates a streaming chat response as an async sequence of chunks.</summary>
    IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct = default);
}
