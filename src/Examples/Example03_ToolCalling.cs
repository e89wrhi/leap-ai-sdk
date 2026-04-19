using Leap.AI.Core;
using Leap.AI.Core.Tools;
using Leap.AI.Providers.OpenAi;

namespace Examples;

public static class Example03_ToolCalling
{
    public record WeatherArgs(string City);

    public static async Task RunAsync(string apiKey)
    {
        Console.WriteLine("\n=== Example 3: Automatic Tool Calling ===");
        
        // Define a Strongly-Typed Tool
        var weatherTool = FunctionTool<WeatherArgs>.Create(
            name: "get_weather",
            description: "Gets the current weather for a specific city.",
            handler: args => 
            {
                Console.WriteLine($"[TOOL EXECUTED]: Getting weather for {args.City}...");
                return $"The current weather in {args.City} is sunny and 22 degrees Celsius.";
            });

        var leap = LeapClient.Create()
            .UseOpenAi(apiKey, "gpt-4o-mini")
            .UseTool(weatherTool)
            .Build();

        Console.WriteLine("[PROMPT]: What's the weather like in Paris right now?");
        var response = await leap.GenerateTextAsync("What's the weather like in Paris right now?");
        
        Console.WriteLine($"[RESPONSE]: {response}");
        Console.WriteLine();
    }
}
