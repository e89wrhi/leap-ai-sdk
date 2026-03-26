using AiSdk.Entities.Common;
using AiSdk.Exceptions;
using AiSdk.Infrastructure;
using System.Net;
using System.Text.Json;

namespace AiSdk;

public class AiClient : IAiClient
{
    private readonly HttpClient _httpClient;

    public AiClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();

        var osVersion = RuntimeInformation.GetOSVersion();
        var netVersion = RuntimeInformation.GetRuntimeVersion();

        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"LeapAiSdk.Net/{Constants.Version.Current}");
        _httpClient.DefaultRequestHeaders.Add("X-AiSdk-Client-User-Agent", JsonSerializer.Serialize(new
        {
            bindings_version = Constants.Version.Current,
            lang = "dotnet",
            publisher = "Leap",
            os_version = osVersion,
            lang_version = netVersion,
            json_version = RuntimeInformation.GetSystemTextJsonVersion()
        }));
    }

    public async Task<T> RequestAsync<T>(HttpMethod method, string path, object options, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, path);
        var json = JsonSerializer.Serialize(options);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, cancellationToken);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseBody)!;
    }

    public async IAsyncEnumerable<T> RequestStreamAsync<T>(HttpMethod method, string path, object options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, path);
        var json = JsonSerializer.Serialize(options);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorAsync(response, cancellationToken);
        }

        using var stream = await response.Content.ReadAsStreamAsync();

        await foreach (var chunk in SseReader.ReadStreamAsync<T>(stream, cancellationToken))
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
            var errorResponse = JsonSerializer.Deserialize<AiErrorResponse>(body);
            error = errorResponse?.Error;
        }
        catch { /* fallback to generic if json ivalid */ }

        var message = error?.Message ?? $"Request failed with status {response.StatusCode}";

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiAuthenticationException(message, response.StatusCode, error, "AiClient"),
            (HttpStatusCode)429 => new AiRateLimitException(message, response.StatusCode, error, "AiClient"),
            _ => new AiSdkException(message, response.StatusCode, error, "AiClient")
        };
    }
}
