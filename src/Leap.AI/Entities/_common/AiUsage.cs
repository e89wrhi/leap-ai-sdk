using AiSdk.Entities.Base;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Common;

public class AiUsage : AiEntity
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("completion_tokens_details")]
    public Dictionary<string, int>? CompletionTokensDetails { get; set; }
}
