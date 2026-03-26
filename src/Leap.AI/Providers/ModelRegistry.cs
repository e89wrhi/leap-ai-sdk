using AiSdk.Providers.Anthropic;
using AiSdk.Providers.Google;
using AiSdk.Providers.OpenAi;

namespace AiSdk.Providers;

public static class ModelRegistry
{
    public static ILanguageModel GetModel(string modelId, string? apiKey = null)
    {
        // Resolution matches string prefixes against the corresponding provider
        apiKey ??= AiConfiguration.ApiKey;

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException($"API Key must be provided either via arguments or AiConfiguration.ApiKey to initialize model '{modelId}'.");
        }

        modelId = modelId.ToLowerInvariant();

        if (modelId.StartsWith("gpt-") || modelId.StartsWith("o1-") || modelId.StartsWith("text-embedding-") || modelId.Contains("dall-e"))
        {
            return new OpenAiModel(modelId, apiKey!, AiConfiguration.HttpClient);
        }

        if (modelId.StartsWith("claude-"))
        {
            return new AnthropicModel(modelId, apiKey!, AiConfiguration.HttpClient);
        }

        if (modelId.StartsWith("gemini-"))
        {
            return new GoogleModel(modelId, apiKey!, AiConfiguration.HttpClient);
        }

        // Fallback for custom or future models, maybe returning a generic OpenAI compatible wrapper
        return new OpenAiModel(modelId, apiKey, AiConfiguration.HttpClient);
    }
}
