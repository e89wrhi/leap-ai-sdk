using System.Text.Json;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;

namespace Leap.AI.Core.Tools;

/// <summary>
/// Wraps a C# delegate as an <see cref="ILeapTool"/>.
/// Automatically generates a JSON Schema from <typeparamref name="TInput"/>.
/// </summary>
/// <typeparam name="TInput">The strongly-typed input POCO the LLM must call this tool with.</typeparam>
public sealed class FunctionTool<TInput> : ILeapTool where TInput : class
{
    private readonly Func<TInput, CancellationToken, Task<string>> _handler;

    public string Name { get; }
    public string Description { get; }
    public JsonElement ParametersSchema { get; }

    public FunctionTool(
        string name,
        string description,
        Func<TInput, CancellationToken, Task<string>> handler)
    {
        Name = name;
        Description = description;
        _handler = handler;
        ParametersSchema = JsonSchemaGenerator.Generate<TInput>();
    }

    /// <summary>Convenience factory for sync handlers.</summary>
    public static FunctionTool<TInput> Create(
        string name,
        string description,
        Func<TInput, string> handler)
        => new(name, description, (input, _) => Task.FromResult(handler(input)));

    /// <summary>Convenience factory for async handlers.</summary>
    public static FunctionTool<TInput> Create(
        string name,
        string description,
        Func<TInput, Task<string>> handler)
        => new(name, description, (input, _) => handler(input));

    public async Task<string> ExecuteAsync(string arguments, CancellationToken ct = default)
    {
        TInput input;
        try
        {
            input = JsonSerializer.Deserialize<TInput>(arguments)
                ?? throw new LeapException($"Tool '{Name}': deserialized input was null.");
        }
        catch (JsonException ex)
        {
            throw new LeapException($"Tool '{Name}': failed to deserialize arguments. {ex.Message}", ex);
        }

        return await _handler(input, ct);
    }
}
