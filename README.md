<p align="center">
  <img src="assets/leap_logo.png" alt="Ai-Logo" width="120">
</p>

# Leap AI SDK

<p align="center">
  <img src="assets/leap_banner.png" alt="Ai Banner" width="100%">
</p>


The **Leap AI SDK** is a provider-agnostic .NET toolkit designed to help you build AI-powered applications, chatbots, and agents. Built with a highly scalable, enterprise-grade architecture, it provides a unified, stateless service interface to interact with any language model.

**AI Models**

<p align="start">
  <img src="assets/models/gpt.png" width="15%" />
  <img src="assets/models/gemini.png" width="15%" />
  <img src="assets/models/deepseek.png" width="15%" />
  <img src="assets/models/grok.png" width="15%" />
</p>

## Installation

You will need the .NET SDK installed on your local development machine.

```shell
dotnet add package Leap.AiSdk
```

## Unified Provider Architecture

The Leap AI SDK provides a unified API to interact with model providers natively from C#.
By default, the SDK delegates execution to provider-adapters (`ILanguageModel`), ensuring that your application code remains completely agnostic to the underlying AI service.

```csharp
using AiSdk.Providers.OpenAi;
using AiSdk.Services.Chat;

// Initialize your provider adapter
var openAiModel = new OpenAiModel("gpt-4o", "sk-...", new HttpClient());

var chatService = new ChatService();

var result = await chatService.CreateAsync(new ChatCreateOptions {
  Model = openAiModel, // or new AnthropicModel("claude-3-opus", ...)
  Messages = new List<ChatMessage> {
      new ChatMessage { Role = "user", Content = "Hello!" }
  }
});
```

---

## Usage

### Generating Text (Chat)

You can generate text and chat completions using the standard `ChatService`. 

```csharp
using AiSdk.Services.Chat;
using AiSdk.Entities.Chat;

var chatService = new ChatService();

var response = await chatService.CreateAsync(new ChatCreateOptions {
  Model = openAiModel,
  Messages = new List<ChatMessage> {
      new ChatMessage { Role = "user", Content = "What is an agent?" }
  }
});

Console.WriteLine(response.Choices[0].Message.Content);
```

### Streaming Text - 🚧 Upcoming

*This feature is currently under active development.*

We are building a `ChatStreamService` that leverages Server-Sent Events (SSE) to map streamed responses into an `IAsyncEnumerable<ChatCompletionChunk>`, allowing you to yield chunks of text to the UI in real-time.

### Generating Structured Data (JSON) - 🚧 Upcoming

*This feature is currently under active development.*

We are developing a `JsonChatService` that enforces the JSON response format from providers and utilizes `System.Text.Json` source generators to perfectly map AI responses directly to strongly-typed C# objects without manual string parsing.

### Agents & Tool Calling - 🚧 Upcoming

*This feature is currently under active development.*

We are bringing dynamic agent capabilities into .NET. You will soon be able to use the `ToolLoopAgent` functionality to seamlessly map your native C# methods to AI tool definitions using Reflection or modern Source Generators.

### UI Integration - 🚧 Upcoming

*This feature is currently under active development.*

The SDK aims to provide a set of middleware components, frameworks, and hubs (e.g., streaming over SignalR) to help you build responsive generative user interfaces across Blazor, ASP.NET Core MVC, and raw minimal APIs.

---

## Community

The Leap AI SDK community can be found on our GitHub repository where you can ask questions, voice ideas, and share your projects with other people.

## Contributing

Contributions to the Leap AI SDK are welcome and highly appreciated. Stay tuned for our Contribution Guidelines to make sure you have a smooth experience contributing!
