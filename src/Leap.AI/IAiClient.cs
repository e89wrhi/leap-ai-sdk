namespace AiSdk;

public interface IAiClient
{
    Task<T> RequestAsync<T>(HttpMethod method, string path, object options, CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> RequestStreamAsync<T>(HttpMethod method, string path, object options, CancellationToken cancellationToken = default);
}
