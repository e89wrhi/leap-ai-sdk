using AiSdk.Entities.Base;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Common;

public class AiError : AiEntity
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }
}

public class AiErrorResponse
{
    [JsonPropertyName("error")]
    public AiError? Error { get; set; }
}
