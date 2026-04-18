using System.Net;
using AiSdk.Entities.Common;
using AiSdk.Infrastructure;

namespace AiSdk;

/// <summary>
/// Low-level HTTP client for the Leap AI SDK v1.x.
/// </summary>
/// <remarks>
/// <strong>Deprecated in v2.0.</strong> Use <c>Leap.AI.Core.LeapClient</c> instead:
/// <code>
/// // v2.0 equivalent
/// var leap = LeapClient.Create()
///     .UseOpenAi("api-key")
///     .UseLogging()
///     .Build();
///
/// var text = await leap.GenerateTextAsync("Hello world");
/// </code>
/// </remarks>
[Obsolete("AiClient is deprecated in v2.0. Use Leap.AI.Core.LeapClient with a provider extension. See migration guide in docs/redesign/migration_roadmap.md.")]
public class AiClient : IAiClient
{
    private readonly HttpClient _httpClient;

    public AiClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();

        var osVersion = RuntimeInformation.GetOSVersion();
        var netVersion = RuntimeInformation.GetRuntimeVersion();

        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"LeapAiSdk.Net/{Constants.Version.Current}");
        _httpClient.DefaultRequestHeaders.Add("X-AiSdk-Client-User-Agent", System.Text.Json.JsonSerializer.Serialize(new
        {
            bindings_version = Constants.Version.Current,
            lang = "dotnet",
            publisher = "Leap",
            os_version = osVersion,
            lang_version = netVersion,
            json_version = RuntimeInformation.GetSystemTextJsonVersion()
        }));
    }

    [Obsolete("Use LeapClient.GenerateAsync() from Leap.AI.Core instead.")]
    public async Task<T> RequestAsync<T>(HttpMethod method, string path, object options, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, path);
        var json = System.Text.Json.JsonSerializer.Serialize(options);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(responseBody)!;
    }

    [Obsolete("Use LeapClient.StreamAsync() from Leap.AI.Core instead.")]
    public async IAsyncEnumerable<T> RequestStreamAsync<T>(HttpMethod method, string path, object options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, path);
        var json = System.Text.Json.JsonSerializer.Serialize(options);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, cancellationToken);
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        await foreach (var chunk in Infrastructure.SseReader.ReadStreamAsync<T>(stream, cancellationToken))
        {
            yield return chunk;
        }
    }

    private async Task HandleErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync();
        AiError? error = null;

        try
        {
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<AiErrorResponse>(body);
            error = errorResponse?.Error;
        }
        catch { /* fallback to generic if json invalid */ }

        var message = error?.Message ?? $"Request failed with status {response.StatusCode}";

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new Exceptions.AiAuthenticationException(message, response.StatusCode, error, "AiClient"),
            (HttpStatusCode)429 => new Exceptions.AiRateLimitException(message, response.StatusCode, error, "AiClient"),
            _ => new Exceptions.AiSdkException(message, response.StatusCode, error, "AiClient")
        };
    }
}
