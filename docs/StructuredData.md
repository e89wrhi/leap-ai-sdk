# Structured Data (JSON) Extraction

One of the most powerful features of Leap AI SDK v2.0 is the ability to generate specific data structures natively from C# types using `GenerateObjectAsync<T>`.

## GenerateObjectAsync

The `GenerateObjectAsync<T>` method automatically parses your C# Records or Classes into standard JSON Schemas and enforces the LLM to output accurate matching JSON, mapped back to the strong type automatically.

```csharp
using Leap.AI.Core;
using Leap.AI.Providers.OpenAi;

// Define your highly nested model
public record CharacterSetup(string Name, string Class, int Level, List<string> Inventory);

var leap = LeapClient.Create().UseOpenAi("sk-...", "gpt-4o").Build();

// Generate it!
CharacterSetup character = await leap.GenerateObjectAsync<CharacterSetup>(
    "Generate a level 5 elven ranger for an RPG, with 3 signature items in their inventory."
);

Console.WriteLine($"Name: {character.Name}");
Console.WriteLine($"Items: {string.Join(", ", character.Inventory)}");
```

## How it Works
1. **Schema Generator**: Leap automatically extracts a complete `json_schema` definition by reflecting on `<T>`. 
2. **Provider Enforcement**: The provider (`OpenAI`, `Anthropic`, `Google`) is given instructions to output JSON exactly matching your defined structure properties natively.
3. **Validation Retry**: (Coming) Built-in resilience middleware attempts deserialization and throws parsing errors for bad structures automatically.

## Tips for Better JSON
- Use descriptive property names to help the AI understand what each field means.
- Small models may struggle with large, highly nested definitions. Use advanced models like `gpt-4o` or `claude-3-5-sonnet` for heavy JSON logic.
