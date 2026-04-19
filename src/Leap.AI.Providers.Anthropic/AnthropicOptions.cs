namespace Leap.AI.Providers.Anthropic;

public sealed class AnthropicOptions
{
    public required string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public string DefaultModel { get; set; } = "claude-3-5-sonnet-latest";
    public string AnthropicVersion { get; set; } = "2023-06-01";
    public HttpClient? HttpClient { get; set; }
}
