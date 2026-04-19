using Examples;

// Set your OpenAI API Key here
const string ApiKey = "sk-...";

Console.WriteLine("Leap AI SDK v2.0 Examples");
Console.WriteLine("=========================");

if (ApiKey == "sk-...")
{
    Console.WriteLine("\n[WARNING]: Please set a valid API key in Program.cs to run the examples.");
    return;
}

await Example01_Streaming.RunAsync(ApiKey);
await Example02_StructuredOutput.RunAsync(ApiKey);
await Example03_ToolCalling.RunAsync(ApiKey);
await Example04_Middleware.RunAsync(ApiKey);

Console.WriteLine("\nAll examples completed.");
