# AI Providers

The Leap AI SDK is designed to be provider-agnostic. You can switch between different AI services by simply swapping the `ILanguageModel` implementation.

## Supported Providers

### OpenAI
The default provider supporting GPT-4, GPT-3.5, and the new O-series models.

```csharp
var model = new OpenAiModel(OpenAiModels.Gpt4o);
```

### Anthropic (Coming Soon)
Support for Claude 3.5 Sonnet, Opus, and Haiku.

### Google (Coming Soon)
Support for Gemini Flash and Pro.

### Custom REST Providers
You can implement your own provider by inheriting from `BaseLanguageModel` and implementing the necessary parsing logic.

## Configuration per Provider

Some providers may require additional configuration (like Organization ID for OpenAI or specific Base URLs for local LLMs).

```csharp
// Example for future versions
var model = new CustomModel("base-url", "custom-path");
```
