using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AiSdk.Entities.Chat;
using AiSdk.Entities.Common;
using AiSdk.Exceptions;
using AiSdk.Services.Chat;
using System.Net;
using System.Text.Json;

namespace AiSdk.Providers;

public abstract class BaseLanguageModel : ILanguageModel
{
    protected readonly HttpClient _httpClient;
    protected readonly string _apiKey;

    public abstract string ProviderName { get; }
    public string ModelId { get; }

    protected BaseLanguageModel(string modelId, string? apiKey = null, HttpClient? httpClient = null)
    {
        var resolvedApiKey = apiKey ?? AiConfiguration.ApiKey;
        if (string.IsNullOrWhiteSpace(resolvedApiKey))
        {
            throw new ArgumentException("API Key cannot be null or empty. Please set it in AiConfiguration or provide it during model initialization.");
        }

        ModelId = modelId;
        _apiKey = resolvedApiKey;
        _httpClient = httpClient ?? AiConfiguration.HttpClient;
    }

    public abstract Task<ChatCompletion> DoGenerateAsync(ChatCreateOptions options, CancellationToken cancellationToken);
    public abstract IAsyncEnumerable<ChatCompletionChunk> DoStreamAsync(ChatCreateOptions options, CancellationToken cancellationToken);

    protected async Task<HttpResponseMessage> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        int totalRetries = AiConfiguration.MaxNetworkRetries;
        int currentRetry = 0;

        while (true)
        {
            var request = requestFactory();
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (ShouldRetry(response.StatusCode) && currentRetry < totalRetries)
            {
                currentRetry++;
                var delay = GetDelay(response, currentRetry);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            return response;
        }
    }

    private bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests // 429
            || statusCode == HttpStatusCode.ServiceUnavailable // 503
            || statusCode == HttpStatusCode.GatewayTimeout; // 504
    }

    private TimeSpan GetDelay(HttpResponseMessage response, int retryCount)
    {
        // Check for Retry-After header
        if (response.Headers.RetryAfter != null)
        {
            if (response.Headers.RetryAfter.Delta.HasValue)
                return response.Headers.RetryAfter.Delta.Value;
            if (response.Headers.RetryAfter.Date.HasValue)
                return response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
        }

        // Exponential backoff
        return TimeSpan.FromSeconds(Math.Pow(2, retryCount));
    }

    protected async Task HandleErrorAsync(HttpResponseMessage response, string provider)
    {
        var errorBody = await response.Content.ReadAsStringAsync();
        AiError? aiError = null;
        try { aiError = JsonSerializer.Deserialize<AiErrorResponse>(errorBody)?.Error; } catch { }

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AiAuthenticationException($"{provider} Authentication Error: {response.StatusCode}", response.StatusCode, aiError, provider),
            HttpStatusCode.TooManyRequests => new AiRateLimitException($"{provider} Rate Limit Error: {response.StatusCode}", response.StatusCode, aiError, provider),
            _ => new AiSdkException($"{provider} Error: {response.StatusCode}", response.StatusCode, aiError, provider)
        };
    }
}
