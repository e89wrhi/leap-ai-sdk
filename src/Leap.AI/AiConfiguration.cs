using AiSdk.Infrastructure;
using System.Text.Json;

namespace AiSdk;

public static class AiConfiguration
{
    public static string? ApiKey { get; set; }
    public static string? OrganizationId { get; set; }
    public static string? ProjectId { get; set; }

    public static HttpClient HttpClient { get; set; } = new HttpClient();
    public static int MaxNetworkRetries { get; set; } = 3;

    public static JsonSerializerOptions SerializerOptions => JsonUtils.DefaultOptions;
    public static JsonSerializerOptions IndentedSerializerOptions => JsonUtils.IndentedOptions;
}
