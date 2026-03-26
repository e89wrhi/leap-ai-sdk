using System.Net;
using System.Net.Http.Headers;

namespace AiSdk.Entities.Common;

public class AiResponse
{
    public HttpStatusCode StatusCode { get; }
    public HttpResponseHeaders Headers { get; }
    public string Content { get; }

    public AiResponse(HttpStatusCode statusCode, HttpResponseHeaders headers, string content)
    {
        StatusCode = statusCode;
        Headers = headers;
        Content = content;
    }
}
