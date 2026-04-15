# Getting Started with Leap AI SDK

Welcome to the Leap AI SDK! This guide will help you get up and running with your first AI-powered application using .NET.

## Installation

The Leap AI SDK is available as a NuGet package. You can install it using the .NET CLI or the NuGet Package Manager.

### .NET CLI
```bash
dotnet add package LeapAi.Sdk
```

### Package Manager
```powershell
Install-Package LeapAi.Sdk
```

## Configuration

Before making requests, you need to configure your API key. You can do this globally:

```csharp
using AiSdk;

AiConfiguration.ApiKey = "your-api-key-here";
```

## Your First Chat Request

The `ChatService` is the primary way to interact with language models.

```csharp
using AiSdk.Providers.OpenAi;
using AiSdk.Services.Chat;
using AiSdk.Entities.Chat;
using AiSdk.Constants;

// 1. Initialize the provider
var model = new OpenAiModel(OpenAiModels.Gpt4oMini);

// 2. Create the chat service
var chatService = new ChatService();

// 3. Send a message
var response = await chatService.CreateAsync(new ChatCreateOptions {
    Model = model,
    Messages = new List<ChatMessage> {
        new ChatMessage { Role = ChatRoles.User, Content = "Hello, what can you do?" }
    }
});

Console.WriteLine(response.Choices[0].Message.Content);
```

## Next Steps

- Explore [Streaming API](./ChatAPI.md) for real-time responses.
- Learn about [Structured Data](./StructuredData.md) for type-safe JSON extraction.
- See supported [Providers](./Providers.md) and models.
