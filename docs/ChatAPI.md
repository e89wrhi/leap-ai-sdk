# Chat & Streaming API

The Leap AI SDK v2.0 provides both synchronous and streaming interfaces for chat completions over a middleware pipeline.

## Generating Text

`GenerateTextAsync` is used for standard request-response cycles.

```csharp
var leap = LeapClient.Create()
    .UseOpenAi("sk-...", "gpt-4o")
    .Build();

var response = await leap.GenerateTextAsync("Hello! Write me a short story.");
Console.WriteLine(response);
```

You can also pass `List<ChatMessage>` objects:
```csharp
var messages = new List<ChatMessage> {
    ChatMessage.System("You are an aggressive pirate."),
    ChatMessage.User("Give me directions to the store.")
};

var response = await leap.GenerateAsync(messages);
```

## Streaming Completions

Streaming allows you to receive tokens as they are generated, providing a more responsive user experience (for UI or console apps).

```csharp
await foreach (var chunk in leap.StreamAsync("Write a 5 paragraph essay on deep learning."))
{
    Console.Write(chunk.Text);
}
```

## Options & Middleware

You can configure options per-pipeline builder:
```csharp
var leap = LeapClient.Create()
    .UseOpenAi("sk-...", "gpt-4o")
    .UseLogging() // Auto-log AI pipeline events
    .UseRetry(maxRetries: 3) // Native exponential backoff
    .Build();
```

## Best Practices
1. **Cancellation**: Pass a `CancellationToken` to `StreamAsync(messages, ct)` to allow UI users to stop long-running generations.
2. **System Prompts**: Always begin a multiple-message chain with `ChatMessage.System()` when instructions are needed.
