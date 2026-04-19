using Leap.AI.Core.Models;

namespace Leap.AI.Core.Abstractions;

/// <summary>
/// Provides type-safe structured output generation from AI models.
/// Automatically generates JSON Schema from C# types and handles retry-on-validation-failure.
/// </summary>
public interface IObjectGenerator
{
    /// <summary>Generates a typed object from a single user prompt.</summary>
    Task<T> GenerateObjectAsync<T>(string prompt, CancellationToken ct = default)
        where T : class;

    /// <summary>Generates a typed object from a list of conversation messages.</summary>
    Task<T> GenerateObjectAsync<T>(IList<ChatMessage> messages, CancellationToken ct = default)
        where T : class;
}
