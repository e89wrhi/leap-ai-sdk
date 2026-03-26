using System.Text.Json.Serialization;

namespace AiSdk.Entities.Chat;

public class Attachment
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }
}
