using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AiSdk.Infrastructure;

public static class SseReader
{
    public static async IAsyncEnumerable<T> ReadStreamAsync<T>(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data: ")) continue;

            var data = line.Substring(6).Trim();

            if (data == "[DONE]") break;

            T? chunk = default;
            try
            {
                chunk = JsonSerializer.Deserialize<T>(data);
            }
            catch (JsonException)
            {
                // Skip malformed chunks or handle partial JSON if necessary
                continue;
            }

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }
}
