using System.Text.Json;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Errors;
using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;
using Leap.AI.Core.Tools;

namespace Leap.AI.Core;

/// <summary>
/// The primary entry point for all AI interactions in Leap AI SDK v2.0.
/// Supports text generation, streaming, structured output, and automatic tool calling.
/// </summary>
/// <example>
/// <code>
/// var leap = LeapClient.Create()
///     .UseOpenAi("sk-...")
///     .UseLogging()
///     .Build();
///
/// // Text generation
/// string text = await leap.GenerateTextAsync("Explain quantum computing");
///
/// // Streaming
/// await foreach (var chunk in leap.StreamAsync("Write a poem"))
///     Console.Write(chunk.Text);
///
/// // Structured output
/// var person = await leap.GenerateObjectAsync&lt;Person&gt;("Generate a fake user profile");
/// </code>
/// </example>
public sealed class LeapClient : IObjectGenerator
{
    private readonly ILeapProvider _provider;
    private readonly MiddlewarePipeline _pipeline;
    private readonly ToolRegistry _toolRegistry;
    private readonly LeapOptions _options;

    internal LeapClient(
        ILeapProvider provider,
        IReadOnlyList<ILeapMiddleware> middlewares,
        ToolRegistry toolRegistry,
        LeapOptions options)
    {
        _provider     = provider;
        _pipeline     = new MiddlewarePipeline(middlewares, provider);
        _toolRegistry = toolRegistry;
        _options      = options;
    }

    /// <summary>Creates a new fluent <see cref="LeapClientBuilder"/>.</summary>
    public static LeapClientBuilder Create() => new();

    // ── Text Generation ───────────────────────────────────────────────────────

    /// <summary>
    /// Generates text from a single user prompt.
    /// Returns the response text string directly.
    /// </summary>
    public async Task<string> GenerateTextAsync(
        string prompt,
        Action<ChatRequest>? configure = null,
        CancellationToken ct = default)
    {
        var response = await GenerateAsync([ChatMessage.User(prompt)], configure, ct);
        return response.Text;
    }

    /// <summary>
    /// Generates a full <see cref="ChatResponse"/> from a list of messages.
    /// Automatically handles tool calls in a loop until the model finishes.
    /// </summary>
    public async Task<ChatResponse> GenerateAsync(
        IList<ChatMessage> messages,
        Action<ChatRequest>? configure = null,
        CancellationToken ct = default)
    {
        var request = BuildRequest(messages, configure);
        return await ExecuteWithToolsAsync(request, ct);
    }

    // ── Streaming ─────────────────────────────────────────────────────────────

    /// <summary>Streams response chunks from a single user prompt.</summary>
    public IAsyncEnumerable<ChatChunk> StreamAsync(
        string prompt,
        Action<ChatRequest>? configure = null,
        CancellationToken ct = default)
        => StreamAsync([ChatMessage.User(prompt)], configure, ct);

    /// <summary>Streams response chunks from a list of messages.</summary>
    public IAsyncEnumerable<ChatChunk> StreamAsync(
        IList<ChatMessage> messages,
        Action<ChatRequest>? configure = null,
        CancellationToken ct = default)
    {
        var request = BuildRequest(messages, configure);
        request.Stream = true;
        return _provider.StreamAsync(request, ct);
    }

    // ── Structured Output ─────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<T> GenerateObjectAsync<T>(string prompt, CancellationToken ct = default)
        where T : class
        => GenerateObjectAsync<T>([ChatMessage.User(prompt)], ct);

    /// <inheritdoc />
    public async Task<T> GenerateObjectAsync<T>(
        IList<ChatMessage> messages,
        CancellationToken ct = default)
        where T : class
    {
        var schema     = JsonSchemaGenerator.Generate<T>();
        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        var systemInstruction = ChatMessage.System(
            $"You must respond with a valid JSON object that exactly matches this JSON Schema:\n" +
            $"```json\n{schemaJson}\n```\n" +
            "Return ONLY the raw JSON object — no prose, no markdown fences, no explanations.");

        var workingMessages = new List<ChatMessage> { systemInstruction };
        workingMessages.AddRange(messages);

        for (var attempt = 0; attempt < _options.MaxStructuredOutputRetries; attempt++)
        {
            var request  = BuildRequest(workingMessages, r => r.ResponseFormat = new { type = "json_object" });
            var response = await _pipeline.ExecuteAsync(request, ct);

            // Strip possible markdown fences the model sneaks in
            var json = response.Text.Trim();
            if (json.StartsWith("```")) json = StripMarkdownFences(json);

            try
            {
                var result = JsonSerializer.Deserialize<T>(json, _options.SerializerOptions);
                if (result is not null) return result;
            }
            catch (JsonException ex)
            {
                if (attempt == _options.MaxStructuredOutputRetries - 1)
                    throw new LeapValidationException(
                        $"Structured output failed after {_options.MaxStructuredOutputRetries} attempts. " +
                        $"Last JSON error: {ex.Message}", inner: ex);

                // Feed back the error for self-correction
                workingMessages.Add(ChatMessage.Assistant(response.Text));
                workingMessages.Add(ChatMessage.User(
                    $"The JSON you returned was invalid. Error: {ex.Message}\n" +
                    "Please correct it and return only the raw JSON object."));
            }
        }

        throw new LeapException("GenerateObjectAsync failed unexpectedly.");
    }

    // ── Tool Registration ─────────────────────────────────────────────────────

    /// <summary>Registers a tool the LLM can invoke. Can be called after Build().</summary>
    public LeapClient RegisterTool(ILeapTool tool)
    {
        _toolRegistry.Register(tool);
        return this;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private async Task<ChatResponse> ExecuteWithToolsAsync(ChatRequest request, CancellationToken ct)
    {
        // Attach registered tools to the request
        if (_toolRegistry.HasTools)
            request.Tools = [.. _toolRegistry.GetDefinitions()];

        var response = await _pipeline.ExecuteAsync(request, ct);

        // Automatic tool-call execution loop
        while (response.ToolCalls?.Count > 0)
        {
            var messages = new List<ChatMessage>(request.Messages)
            {
                ChatMessage.Assistant(response.Text)
            };

            foreach (var call in response.ToolCalls)
            {
                var tool = _toolRegistry.Get(call.Function.Name);
                if (tool is null)
                {
                    messages.Add(ChatMessage.ToolResult(
                        $"Error: Tool '{call.Function.Name}' is not registered.", call.Id));
                    continue;
                }

                try
                {
                    var result = await tool.ExecuteAsync(call.Function.Arguments, ct);
                    messages.Add(ChatMessage.ToolResult(result, call.Id));
                }
                catch (Exception ex)
                {
                    messages.Add(ChatMessage.ToolResult($"Tool error: {ex.Message}", call.Id));
                }
            }

            request.Messages = messages;
            response = await _pipeline.ExecuteAsync(request, ct);
        }

        return response;
    }

    private ChatRequest BuildRequest(IList<ChatMessage> messages, Action<ChatRequest>? configure)
    {
        var all = new List<ChatMessage>();

        if (_options.DefaultSystemPrompt is not null)
            all.Add(ChatMessage.System(_options.DefaultSystemPrompt));

        all.AddRange(messages);

        var request = new ChatRequest
        {
            Messages    = all,
            Temperature = _options.DefaultTemperature,
            MaxTokens   = _options.DefaultMaxTokens,
        };

        configure?.Invoke(request);
        return request;
    }

    private static string StripMarkdownFences(string text)
    {
        var lines = text.Split('\n');
        var start = Array.FindIndex(lines, l => l.TrimStart().StartsWith("```"));
        var end   = Array.FindLastIndex(lines, l => l.Trim() == "```");
        if (start >= 0 && end > start)
            return string.Join('\n', lines[(start + 1)..end]);
        return text;
    }
}
