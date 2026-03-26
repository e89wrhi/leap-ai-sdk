using System.Text.Json.Serialization;

namespace AiSdk.Entities.Base;

public class AiList<T> : AiEntity
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
