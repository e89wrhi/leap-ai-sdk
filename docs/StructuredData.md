# Structured Data (JSON) Extraction

One of the most powerful features of Leap AI is the ability to map AI responses directly to C# classes or records.

## ChatJsonService

The `ChatJsonService` ensures the LLM output is valid JSON and automatically deserializes it into your target type.

```csharp
using AiSdk.Services.ChatJson;

// Define your model
public record BookInfo(string Title, string Author, List<string> Themes);

var jsonService = new ChatJsonService();
var options = new ChatCreateOptions {
    Model = model,
    Messages = new List<ChatMessage> {
        new ChatMessage { Role = ChatRoles.User, Content = "Analyze 'The Great Gatsby'." }
    }
};

BookInfo info = await jsonService.CreateObjectAsync<BookInfo>(options);
Console.WriteLine($"Author: {info.Author}");
```

## How it Works
1. **Schema Generation**: Leap automatically generates a JSON Schema from your C# type.
2. **Strict Mode**: It instructs the LLM to adhere strictly to that schema.
3. **Validation**: The SDK validates the response before returning the object to you.

## Tips for Better JSON
- Use Descriptive Property Names: Help the AI understand what each field represents.
- Use `Required` or `DefaultValue` if needed via attributes.
- Keep schemas simple for smaller models; larger models can handle complex nested objects.
