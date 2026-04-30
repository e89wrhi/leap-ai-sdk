using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;
using Leap.AI.Providers.OpenAi;

namespace Leap.AI.Providers.xAI;

/// <summary>
/// Leap AI provider for xAI's Grok models.
/// <para>
/// xAI's API is fully compatible with the OpenAI Chat Completions specification,
/// so this provider delegates all HTTP communication to <see cref="OpenAiProvider"/>
/// with xAI-specific defaults (base URL and model names).
/// Full streaming, tool calling, and structured output are supported.
/// </para>
/// </summary>
/// <remarks>
/// Supported models: <c>grok-3</c>, <c>grok-3-fast</c>, <c>grok-3-mini</c>,
/// <c>grok-3-mini-fast</c>, <c>grok-2</c>.
/// </remarks>
public sealed class XAiProvider : ILeapProvider
{
    private readonly OpenAiProvider _inner;

    /// <inheritdoc />
    public string Name => "xai";

    /// <summary>
    /// Initializes a new <see cref="XAiProvider"/> from the given <paramref name="options"/>.
    /// </summary>
    public XAiProvider(XAiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // xAI is OpenAI-API-compatible: reuse OpenAiProvider with xAI-specific settings.
        _inner = new OpenAiProvider(new OpenAiOptions
        {
            ApiKey       = options.ApiKey,
            BaseUrl      = options.BaseUrl,
            DefaultModel = options.DefaultModel,
            HttpClient   = options.HttpClient,
        });
    }

    /// <inheritdoc />
    public Task<ChatResponse> GenerateAsync(ChatRequest request, CancellationToken ct = default)
        => _inner.GenerateAsync(request, ct);

    /// <inheritdoc />
    public IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct = default)
        => _inner.StreamAsync(request, ct);
}
