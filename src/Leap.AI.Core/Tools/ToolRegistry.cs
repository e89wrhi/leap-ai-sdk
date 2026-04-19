using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;

namespace Leap.AI.Core.Tools;

/// <summary>
/// Registry of <see cref="ILeapTool"/> instances available to the <see cref="LeapClient"/>.
/// Provides definitions in the format required by AI providers.
/// </summary>
public sealed class ToolRegistry
{
    private readonly Dictionary<string, ILeapTool> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ILeapTool tool) => _tools[tool.Name] = tool;

    public ILeapTool? Get(string name) =>
        _tools.TryGetValue(name, out var t) ? t : null;

    public IEnumerable<ToolDefinition> GetDefinitions() =>
        _tools.Values.Select(t => new ToolDefinition
        {
            Function = new FunctionDefinition
            {
                Name        = t.Name,
                Description = t.Description,
                Parameters  = t.ParametersSchema,
            }
        });

    public bool HasTools => _tools.Count > 0;
    public int Count     => _tools.Count;
}
