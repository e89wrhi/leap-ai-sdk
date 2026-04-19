# Getting Started with Leap AI SDK v2.0

Welcome to the Leap AI SDK v2.0! This guide will help you get up and running with your first AI-powered application using .NET.

## Installation

The Leap AI SDK is available as a NuGet package. You can install it using the .NET CLI or the NuGet Package Manager.

### .NET CLI
```bash
dotnet add package LeapAi.Sdk
```

*Note: As this is currently in Prerelease, you might need to append `--prerelease` flag.*

## Your First Chat Request

Leap AI v2 is built around a fluent **pipeline builder** (`LeapClient.Create()`).

```csharp
using Leap.AI.Core;
using Leap.AI.Providers.OpenAi;

// 1. Configure the pipeline and provider
var leap = LeapClient.Create()
    .UseOpenAi("your-api-key-here", "gpt-4o-mini")
    .Build();

// 2. Generate a response!
string response = await leap.GenerateTextAsync("Hello, what can you do?");

Console.WriteLine(response);
```

## Next Steps

- Explore [Chat API](./ChatAPI.md) for real-time streaming constraints.
- Learn about [Structured Data](./StructuredData.md) for type-safe JSON extraction.
- See supported [Providers](./Providers.md) and models.
