using Leap.AI.Core;
using Leap.AI.Providers.OpenAi;

namespace Examples;

public static class Example02_StructuredOutput
{
    public record CharacterInfo(string Name, string Class, int Level, List<string> Inventory);

    public static async Task RunAsync(string apiKey)
    {
        Console.WriteLine("\n=== Example 2: Structured JSON Object Generation ===");
        var leap = LeapClient.Create()
            .UseOpenAi(apiKey, "gpt-4o-mini")
            .Build();

        var prompt = "Generate a level 5 elven ranger for an RPG, with 3 signature items in their inventory.";
        var character = await leap.GenerateObjectAsync<CharacterInfo>(prompt);
            
        Console.WriteLine($"- Name: {character.Name}");
        Console.WriteLine($"- Class: {character.Class} (Level {character.Level})");
        Console.WriteLine($"- Inventory: {string.Join(", ", character.Inventory)}");
        Console.WriteLine();
    }
}
