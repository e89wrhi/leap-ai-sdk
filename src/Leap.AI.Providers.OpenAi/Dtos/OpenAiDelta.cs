using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Dtos;

internal sealed class OpenAiDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; init; }
}
