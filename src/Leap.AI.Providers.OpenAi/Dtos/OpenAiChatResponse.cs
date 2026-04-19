using System.Text.Json;
using System.Text.Json.Serialization;
using Leap.AI.Core.Models;

namespace Leap.AI.Providers.OpenAi.Dtos;

internal sealed class OpenAiChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<OpenAiChoice> Choices { get; init; } = [];

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; init; }
}
