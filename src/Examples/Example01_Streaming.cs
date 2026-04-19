using Leap.AI.Core;
using Leap.AI.Core.Models;
using Leap.AI.Providers.OpenAi;

namespace Examples;

public static class Example01_Streaming
{
    public static async Task RunAsync(string apiKey)
    {
        Console.WriteLine("\n=== Example 1: Streaming Chat Response ===");
        var leap = LeapClient.Create()
            .UseOpenAi(apiKey, "gpt-4o-mini")
            .Build();

        var messages = new List<ChatMessage>
        {
            ChatMessage.System("You are a poetic assistant."),
            ChatMessage.User("Write a short poem about the future of AI."),
        };

        Console.Write("[RESPONSE]: ");
        await foreach (var chunk in leap.StreamAsync(messages))
        {
            Console.Write(chunk.Text);
        }
        Console.WriteLine("\n");
    }
}
