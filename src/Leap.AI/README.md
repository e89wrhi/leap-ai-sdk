# Leap AI SDK

The **Leap AI SDK** is a provider-agnostic .NET toolkit designed to help you build AI-powered applications, chatbots, and agents. Built with a highly scalable, enterprise-grade architecture, it provides a unified, stateless service interface to interact with any language model.

## Installation

You will need the .NET SDK installed on your local development machine.

```shell
dotnet add package Leap.AiSdk
```

## Unified Provider Architecture

The Leap AI SDK provides a unified API to interact with model providers natively from C#.
By default, the SDK delegates execution to provider-adapters (`ILanguageModel`), ensuring that your application code remains completely agnostic to the underlying AI service.

```csharp
using AiSdk;
using AiSdk.Constants;
using AiSdk.Entities.Chat;
using AiSdk.Providers.OpenAi;
using AiSdk.Services.Chat;

// Set API Key globally
AiConfiguration.ApiKey = "sk-...";

// Initialize your provider adapter
var openAiModel = new OpenAiModel(OpenAiModels.Gpt4oMini); // or new AnthropicModel(...)

var chatService = new ChatService();

var result = await chatService.CreateAsync(new ChatCreateOptions {
  Model = openAiModel,
  Messages = new List<ChatMessage> {
      new ChatMessage { Role = ChatRoles.User, Content = "Hello!" }
  }
});
```

---

## Usage

### Generating Text (Chat)

You can generate text and chat completions using the standard `ChatService`. 

```csharp
var options = new ChatCreateOptions {
  Model = openAiModel,
  Messages = new List<ChatMessage> {
      new ChatMessage { Role = ChatRoles.System, Content = "You are a helpful assistant." },
      new ChatMessage { Role = ChatRoles.User, Content = "What is an agent?" }
  }
};

var response = await chatService.CreateAsync(options);
Console.WriteLine(response.Choices[0].Message.Content);
```

### Streaming Text

The SDK natively leverages Server-Sent Events (SSE) to map streamed responses into an `IAsyncEnumerable<ChatCompletionChunk>`, allowing you to yield chunks of text to the UI in real-time.

```csharp
var options = new ChatCreateOptions
{
    Model = openAiModel,
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = ChatRoles.User, Content = "Count to 3 quickly." }
    }
};

await foreach (var chunk in chatService.StreamAsync(options))
{
    if (chunk.Choices is { Count: > 0 } && chunk.Choices[0].Delta?.Content != null)
    {
        Console.Write(chunk.Choices[0].Delta.Content);
    }
}
```

### Generating Structured Data (JSON)

The `ChatJsonService` enforces accurate JSON output from providers and maps the AI response directly into your strongly-typed C# objects automatically without manual string parsing.

```csharp
using AiSdk.Services.ChatJson;

public record Recipe(string Name, int PrepTimeMinutes, List<string> Ingredients);

var chatJsonService = new ChatJsonService();
var jsonOptions = new ChatCreateOptions
{
    Model = openAiModel,
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = ChatRoles.System, Content = "You extract data into JSON formatted strictly to the requested structure." },
        new ChatMessage { Role = ChatRoles.User, Content = "Generate a simple chocolate chip cookie recipe." }
    }
};

var recipe = await chatJsonService.CreateObjectAsync<Recipe>(jsonOptions);
Console.WriteLine($"Recipe: {recipe.Name} ({recipe.PrepTimeMinutes}m prep)");


### Agents & Tool Calling - 🚧 Upcoming

*This feature is currently under active development.*

We are bringing dynamic agent capabilities into .NET. You will soon be able to use the `ToolLoopAgent` functionality to seamlessly map your native C# methods to AI tool definitions using Reflection or modern Source Generators.

---

## Community

The Leap AI SDK community can be found on our GitHub repository where you can ask questions, voice ideas, and share your projects with other people.

## Contributing

Contributions to the Leap AI SDK are welcome and highly appreciated. Stay tuned for our Contribution Guidelines to make sure you have a smooth experience contributing!
