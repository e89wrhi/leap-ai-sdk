using System.Text.Json;
using Leap.AI.Core;
using Leap.AI.Core.Abstractions;
using Leap.AI.Core.Models;
using Leap.AI.Core.Tools;
using Moq;
using Xunit;

namespace LeapTests;

public class LeapClientTests
{
    [Fact]
    public async Task GenerateTextAsync_SendsCorrectRequestAndReturnsText()
    {
        // Arrange
        var mockProvider = new Mock<ILeapProvider>();
        mockProvider
            .Setup(p => p.GenerateAsync(It.IsAny<ChatRequest>(), default))
            .ReturnsAsync(new ChatResponse { Text = "Mock response text" });

        var client = LeapClient.Create()
            .WithProvider(mockProvider.Object)
            .Build();

        // Act
        var result = await client.GenerateTextAsync("Test prompt");

        // Assert
        Assert.Equal("Mock response text", result);
        mockProvider.Verify(p => p.GenerateAsync(It.Is<ChatRequest>(r => 
            r.Messages.Count == 1 && r.Messages[0].Content == "Test prompt"), default), 
            Times.Once);
    }

    [Fact]
    public async Task GenerateObjectAsync_HandlesJsonSchemaAndValidates()
    {
        // Arrange
        var mockProvider = new Mock<ILeapProvider>();
        var jsonResponse = """{"Name": "Test", "Value": 42}""";
        
        mockProvider
            .Setup(p => p.GenerateAsync(It.IsAny<ChatRequest>(), default))
            .ReturnsAsync(new ChatResponse { Text = jsonResponse });

        var client = LeapClient.Create()
            .WithProvider(mockProvider.Object)
            .Build();

        // Act
        var result = await client.GenerateObjectAsync<TestRecord>("Generate test object");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task ToolCalling_AutoExecutesRegisteredTools()
    {
        // Arrange
        var mockProvider = new Mock<ILeapProvider>();
        
        // Initial call asks for a tool
        mockProvider
            .SetupSequence(p => p.GenerateAsync(It.IsAny<ChatRequest>(), default))
            .ReturnsAsync(new ChatResponse 
            { 
                Text = "", 
                ToolCalls = new List<ToolCallResult> 
                { 
                    new ToolCallResult 
                    { 
                        Id = "call_1", 
                        Function = new FunctionCallResult 
                        { 
                            Name = "get_weather", 
                            Arguments = """{"City": "Paris"}""" 
                        } 
                    } 
                } 
            })
            // Second call provides the final answer
            .ReturnsAsync(new ChatResponse { Text = "The weather in Paris is sunny." });

        var tool = FunctionTool<WeatherArgs>.Create("get_weather", "Returns weather", a => "Sunny in " + a.City);

        var client = LeapClient.Create()
            .WithProvider(mockProvider.Object)
            .UseTool(tool)
            .Build();

        // Act
        var response = await client.GenerateAsync(new List<ChatMessage> { ChatMessage.User("Weather in Paris?") });

        // Assert
        Assert.Equal("The weather in Paris is sunny.", response.Text);
        mockProvider.Verify(p => p.GenerateAsync(It.IsAny<ChatRequest>(), default), Times.Exactly(2));
    }

    private record TestRecord(string Name, int Value);
    private record WeatherArgs(string City);
}
