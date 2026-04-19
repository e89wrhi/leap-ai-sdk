using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Dtos;

internal sealed class OpenAiTool
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    [JsonPropertyName("function")]
    public OpenAiFunctionDef Function { get; init; } = null!;
}
