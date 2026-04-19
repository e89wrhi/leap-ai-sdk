using Leap.AI.Core;
using Leap.AI.Providers.OpenAi;

namespace Examples;

public static class Example04_Middleware
{
    public static async Task RunAsync(string apiKey)
    {
        Console.WriteLine("\n=== Example 4: Middleware and Event Logging ===");
        var leap = LeapClient.Create()
            .UseOpenAi(apiKey, "gpt-4o-mini")
            .UseLogging() // Logs request metadata natively
            .UseRetry(maxRetries: 3) // Automatically retries transient errors
            .UseMiddleware((context, next, ct) => 
            {
                Console.WriteLine($"[CUSTOM MIDDLEWARE] Intercepting request to provider: {context.Provider.Name}");
                return next(context);
            })
            .Build();

        var response = await leap.GenerateTextAsync("Tell me a quick joke.");
        Console.WriteLine($"[RESPONSE]: {response}");
        Console.WriteLine();
    }
}
