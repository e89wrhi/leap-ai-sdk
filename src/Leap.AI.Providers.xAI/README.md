<p align="center">
  <img src="assets/leap_logo.png" alt="Leap AI Logo" width="120">
</p>

# LeapAi.SDK.Providers.xAI

<p align="center">
  <a href="https://www.nuget.org/packages/LeapAi.SDK.Providers.xAI"><img src="https://img.shields.io/nuget/v/LeapAi.SDK.Providers.xAI.svg" alt="NuGet" /></a>
</p>

xAI **Grok** provider for the [Leap AI SDK](https://www.nuget.org/packages/LeapAi.SDK) — a provider-agnostic .NET toolkit for building AI-powered applications, chatbots, and agents against a unified `LeapClient`.

## Installation

```shell
dotnet add package LeapAi.SDK.Providers.xAI
```

You will also need the Core package (pulled in automatically as a transitive dependency):

```shell
dotnet add package LeapAi.SDK.Core
```

## Quick Start

```csharp
using Leap.AI.Core;
using Leap.AI.Providers.xAI;

var leap = LeapClient.Create()
    .UseXAi("xai-...")          // grok-3 by default
    .UseLogging()
    .UseRetry(maxRetries: 3)
    .Build();

string result = await leap.GenerateTextAsync("Explain the Fermi paradox.");
Console.WriteLine(result);
```

## Supported Models

| Model | Description |
|---|---|
| `grok-3` *(default)* | Most capable — best reasoning and instruction-following |
| `grok-3-fast` | Speed-optimised variant of Grok 3 |
| `grok-3-mini` | Lightweight, cost-efficient |
| `grok-3-mini-fast` | Fastest and smallest |
| `grok-2` | Previous-generation model |

Override the model at build time:

```csharp
var leap = LeapClient.Create()
    .UseXAi("xai-...", defaultModel: "grok-3-fast")
    .Build();
```

Or via the options delegate:

```csharp
var leap = LeapClient.Create()
    .UseXAi(o =>
    {
        o.ApiKey       = "xai-...";
        o.DefaultModel = "grok-3-mini";
    })
    .Build();
```

## Usage

### Text Generation

```csharp
string answer = await leap.GenerateTextAsync("What is a transformer model?");
```

### Conversation History

```csharp
var messages = new List<ChatMessage>
{
    ChatMessage.System("You are a concise technical assistant."),
    ChatMessage.User("What is xAI's Grok?")
};

var response = await leap.GenerateAsync(messages);
Console.WriteLine(response.Text);
```

### Streaming

```csharp
await foreach (var chunk in leap.StreamAsync("Write a haiku about AGI."))
{
    Console.Write(chunk.Text);
}
```

### Structured Output

```csharp
public record MarketSummary(string Ticker, double Price, string Sentiment);

var summary = await leap.GenerateObjectAsync<MarketSummary>(
    "Summarise the current state of $TSLA in one concise JSON object."
);

Console.WriteLine($"{summary.Ticker}: ${summary.Price} — {summary.Sentiment}");
```

### Tool Calling (Agents)

```csharp
using Leap.AI.Core.Tools;

public record SearchArgs(string Query);

var searchTool = FunctionTool<SearchArgs>.Create(
    name: "web_search",
    description: "Searches the web and returns a summary of the top results.",
    handler: args => $"Top result for '{args.Query}': ..."
);

var leap = LeapClient.Create()
    .UseXAi("xai-...")
    .UseTool(searchTool)
    .Build();

var response = await leap.GenerateTextAsync("Search for the latest xAI news.");
```

## ASP.NET Core / Dependency Injection

Install the DI extension package:

```shell
dotnet add package LeapAi.SDK.Extensions.DependencyInjection
```

```csharp
using Leap.AI.Extensions.DependencyInjection;

builder.Services.AddLeap(leap => leap
    .UseXAi(builder.Configuration["xAI:ApiKey"]!)
    .UseLogging()
    .UseRetry(3));
```

Inject `LeapClient` anywhere in your app:

```csharp
public class MyService(LeapClient ai)
{
    public Task<string> AskAsync(string q) => ai.GenerateTextAsync(q);
}
```

## Available Packages

| Package | Purpose |
|---|---|
| `LeapAi.SDK` | All-in-one metapackage |
| `LeapAi.SDK.Core` | Core abstractions & pipeline |
| `LeapAi.SDK.Providers.OpenAi` | OpenAI (GPT-4o, o3-mini …) |
| `LeapAi.SDK.Providers.Anthropic` | Anthropic (Claude 3.5/3.7 …) |
| `LeapAi.SDK.Providers.Google` | Google (Gemini 2.0 Flash …) |
| **`LeapAi.SDK.Providers.xAI`** | **xAI (Grok 3 …)** |
| `LeapAi.SDK.Extensions.DependencyInjection` | ASP.NET Core DI |

---

## Community & Contributing

The Leap AI SDK community lives on our GitHub repository. Questions, ideas, and contributions are all welcome!
