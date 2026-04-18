using Leap.AI.Core;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;
using Leap.AI.Core.Pipeline;
using Moq;
using Xunit;

namespace LeapTests;

public class MiddlewarePipelineTests
{
    [Fact]
    public async Task PipelineExecutesInOrder()
    {
        // Arrange
        var events = new List<string>();
        
        var mw1 = new Mock<ILeapMiddleware>();
        mw1.Setup(m => m.InvokeAsync(It.IsAny<ChatContext>(), It.IsAny<Func<ChatContext, Task<ChatResponse>>>(), default))
            .Returns(async (ChatContext ctx, Func<ChatContext, Task<ChatResponse>> next, CancellationToken ct) => 
            {
                events.Add("mw1-in");
                var res = await next(ctx);
                events.Add("mw1-out");
                return res;
            });

        var mw2 = new Mock<ILeapMiddleware>();
        mw2.Setup(m => m.InvokeAsync(It.IsAny<ChatContext>(), It.IsAny<Func<ChatContext, Task<ChatResponse>>>(), default))
            .Returns(async (ChatContext ctx, Func<ChatContext, Task<ChatResponse>> next, CancellationToken ct) => 
            {
                events.Add("mw2-in");
                var res = await next(ctx);
                events.Add("mw2-out");
                return res;
            });

        var mockProvider = new Mock<ILeapProvider>();
        mockProvider.Setup(p => p.GenerateAsync(It.IsAny<ChatRequest>(), default))
            .ReturnsAsync(() => 
            {
                events.Add("provider");
                return new ChatResponse { Text = "done" };
            });

        var client = LeapClient.Create()
            .WithProvider(mockProvider.Object)
            .UseMiddleware(mw1.Object)
            .UseMiddleware(mw2.Object)
            .Build();

        // Act
        await client.GenerateTextAsync("Test");

        // Assert
        Assert.Equal(new[] { "mw1-in", "mw2-in", "provider", "mw2-out", "mw1-out" }, events);
    }
}
