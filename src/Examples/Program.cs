using AiSdk;
using AiSdk.Constants;
using AiSdk.Entities.Chat;
using AiSdk.Providers.OpenAi;
using AiSdk.Services.Chat;
using AiSdk.Services.ChatJson;

// Set API Key
const string ApiKey = "sk-";
AiConfiguration.ApiKey = ApiKey;

// Initialize Service and Model
var openAiModel = new OpenAiModel(OpenAiModels.Gpt4oMini);
var chatService = new ChatService();
var chatJsonService = new ChatJsonService();

// ==========================================
// EXAMPLE Streaming Chat Response
// ==========================================
Console.WriteLine($"[INFO] Example 1: Streaming request to {openAiModel.ProviderName} ({openAiModel.ModelId})...");
try 
{
    var options = new ChatCreateOptions
    {
        Model = openAiModel,
        Messages = new List<ChatMessage>
        {
            new ChatMessage { Role = ChatRoles.System, Content = "You are a very polite assistant." },
            new ChatMessage { Role = ChatRoles.User, Content = "Hello, please count to 3 quickly." },
        }
    };

    Console.Write("[RESPONSE]: ");
    await foreach (var chunk in chatService.StreamAsync(options))
    {
        if (chunk.Choices is { Count: > 0 } && chunk.Choices[0].Delta?.Content != null)
        {
            Console.Write(chunk.Choices[0].Delta.Content);
        }
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"\n[FATAL ERROR]: {ex.Message}");
}

/*
// ==========================================
// EXAMPLE Structured JSON Generator
// ==========================================
Console.WriteLine($"\n[INFO] Example 2: Structured JSON object generation requesting a recipe...");
try
{
    var jsonOptions = new ChatCreateOptions
    {
        Model = openAiModel,
        Messages = new List<ChatMessage>
        {
            new ChatMessage { Role = ChatRoles.System, Content = "You extract data into JSON formatted strictly to the requested structure." },
            new ChatMessage { Role = ChatRoles.User, Content = "Generate a simple chocolate chip cookie recipe." }
        }
    };

    var recipe = await chatJsonService.CreateObjectAsync<Recipe>(jsonOptions);
    Console.WriteLine("Successfully deserialized entity:");
    Console.WriteLine($"- Recipe Name: {recipe.Name}");
    Console.WriteLine($"- Prep Time: {recipe.PrepTimeMinutes} minutes");
    Console.WriteLine($"- Ingredients: {string.Join(", ", recipe.Ingredients)}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[FATAL ERROR]: {ex.Message}");
}

Console.WriteLine("\nDone.");

// Record used for the JSON deserialization example
public record Recipe(string Name, int PrepTimeMinutes, List<string> Ingredients);
*/
