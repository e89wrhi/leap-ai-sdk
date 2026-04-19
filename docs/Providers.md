# AI Providers

The Leap AI SDK v2.0 is designed from the ground up to be fully provider-agnostic. You can switch between different AI services by simply swapping the provider configuration in the pipeline builder.

## Supported Providers

### OpenAI
The default provider supporting GPT-4o, GPT-3.5, and the O-series models.

```csharp
var leap = LeapClient.Create()
    .UseOpenAi("api-key", "gpt-4o")
    .Build();
```

### Anthropic
Native support for Claude 3.5 Sonnet, Opus, and Haiku.

```csharp
var leap = LeapClient.Create()
    .UseAnthropic("api-key", "claude-3-5-sonnet-20240620")
    .Build();
```

### Google Gemini
Native support for Gemini 1.5 Flash and Pro.

```csharp
var leap = LeapClient.Create()
    .UseGoogle("api-key", "gemini-1.5-flash")
    .Build();
```

## Custom REST Providers

To implement your own provider, simply create a class that implements `ILeapProvider` from the `Leap.AI.Core.Abstractions` project.

You will need to implement a single method:
```csharp
Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken cancellationToken = default);
```
