using AiSdk;
using AiSdk.Exceptions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace LeapTests;

public class AiClientTests
{
    private class TestResponse
    {
        public string Result { get; set; } = "";
    }

    private static HttpClient CreateMockedHttpClient(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    [Fact]
    public async Task RequestAsync_SuccessfulResponse_ReturnsDeserializedObject()
    {
        // Arrange
        var expectedResponse = new TestResponse { Result = "Success" };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        var httpClient = CreateMockedHttpClient(HttpStatusCode.OK, jsonResponse);
        var client = new AiClient(httpClient);

        // Act
        var result = await client.RequestAsync<TestResponse>(HttpMethod.Post, "http://localhost/test", new { });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Success", result.Result);
    }

    [Fact]
    public async Task RequestAsync_UnauthorizedResponse_ThrowsAiAuthenticationException()
    {
        // Arrange
        var httpClient = CreateMockedHttpClient(HttpStatusCode.Unauthorized, "{}");
        var client = new AiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<AiAuthenticationException>(() =>
            client.RequestAsync<TestResponse>(HttpMethod.Post, "http://localhost/test", new { }));
    }

    [Fact]
    public async Task RequestAsync_RateLimitResponse_ThrowsAiRateLimitException()
    {
        // Arrange
        var httpClient = CreateMockedHttpClient((HttpStatusCode)429, "{}");
        var client = new AiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<AiRateLimitException>(() =>
            client.RequestAsync<TestResponse>(HttpMethod.Post, "http://localhost/test", new { }));
    }

    [Fact]
    public async Task RequestAsync_OtherErrorResponse_ThrowsAiSdkException()
    {
        // Arrange
        var httpClient = CreateMockedHttpClient(HttpStatusCode.InternalServerError, "{}");
        var client = new AiClient(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<AiSdkException>(() =>
            client.RequestAsync<TestResponse>(HttpMethod.Post, "http://localhost/test", new { }));
    }
}
