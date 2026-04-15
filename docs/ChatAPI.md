# Chat & Streaming API

The Leap AI SDK provides both synchronous and streaming interfaces for chat completions.

## ChatService

`ChatService` is used for standard request-response cycles.

### Methods
- `CreateAsync(ChatCreateOptions options, CancellationToken ct = default)`: Returns a `Task<ChatCompletion>`.

### Options
- `Model`: The `ILanguageModel` instance (e.g., `OpenAiModel`).
- `Messages`: A list of `ChatMessage` objects.
- `Temperature`: Sampling temperature (0.0 to 2.0).
- `MaxTokens`: Limits the response length.

## Streaming Completions

Streaming allows you to receive tokens as they are generated, providing a more responsive user experience.

```csharp
var options = new ChatCreateOptions {
    Model = model,
    Messages = new List<ChatMessage> {
        new ChatMessage { Role = ChatRoles.User, Content = "Write a short story." }
    }
};

await foreach (var chunk in chatService.StreamAsync(options))
{
    var content = chunk.Choices[0].Delta?.Content;
    if (!string.IsNullOrEmpty(content))
    {
        Console.Write(content);
    }
}
```

## Best Practices
1. **Cancellation**: Always pass a `CancellationToken` to `CreateAsync` and `StreamAsync` to allow users to stop long-running generations.
2. **Error Handling**: Wrap calls in try-catch blocks to handle `AiSdkException` or network timeouts.
