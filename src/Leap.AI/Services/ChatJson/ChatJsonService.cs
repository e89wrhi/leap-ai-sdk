using AiSdk.Services.Chat;
using System.Text.Json;

namespace AiSdk.Services.ChatJson;

public class ChatJsonService
{
    private readonly ChatService _chatService;

    public ChatJsonService()
    {
        _chatService = new ChatService();
    }

    public async Task<T> CreateObjectAsync<T>(ChatCreateOptions options, CancellationToken cancellationToken = default)
    {
        // Enforce response format down to the provider adapters
        options.ResponseFormat = new { type = "json_object" };

        var chatResponse = await _chatService.CreateAsync(options, cancellationToken);

        var content = chatResponse.Choices?[0]?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new Exceptions.AiSdkException("Failed to generate an object. The language model returned an empty response.");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(content, AiConfiguration.SerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new Exceptions.AiSdkException($"Failed to deserialize the JSON object from the model. Original content: {content}", ex);
        }
    }
}
