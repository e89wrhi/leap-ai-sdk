using System.Text.Json;
using Leap.AI.Core.Tools;
using Xunit;

namespace LeapTests;

public class JsonSchemaGeneratorTests
{
    private enum Role { User, Admin }
    
    private record UserProfile(
        string Username,
        int Age,
        Role UserRole,
        List<string> Tags,
        bool? IsActive
    );

    [Fact]
    public void Generate_CreatesValidSchemaForComplexType()
    {
        // Act
        var schemaElement = JsonSchemaGenerator.Generate<UserProfile>();
        var json = JsonSerializer.Serialize(schemaElement);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        
        Assert.Equal("object", root.GetProperty("type").GetString());
        var props = root.GetProperty("properties");
        
        // Checks that strings map correctly
        Assert.Equal("string", props.GetProperty("username").GetProperty("type").GetString());
        
        // Checks integers
        Assert.Equal("integer", props.GetProperty("age").GetProperty("type").GetString());
        
        // Checks enum
        var roleEnum = props.GetProperty("userRole");
        Assert.Equal("string", roleEnum.GetProperty("type").GetString());
        Assert.Equal(2, roleEnum.GetProperty("enum").GetArrayLength());
        
        // Checks List<T> mapped to array
        var tags = props.GetProperty("tags");
        Assert.Equal("array", tags.GetProperty("type").GetString());
        Assert.Equal("string", tags.GetProperty("items").GetProperty("type").GetString());
        
        // Check Required fields (IsActive is boolean? null, shouldn't be required)
        var required = root.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("username", required);
        Assert.Contains("age", required);
        Assert.DoesNotContain("isActive", required);
    }

    private record CollectionTest(IEnumerable<int> Numbers);

    [Fact]
    public void Generate_HandlesIEnumerableProperly()
    {
        // Act
        var schemaElement = JsonSchemaGenerator.Generate<CollectionTest>();
        var json = JsonSerializer.Serialize(schemaElement);

        // Assert
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        
        var props = root.GetProperty("properties");
        var numbers = props.GetProperty("numbers");
        Assert.Equal("array", numbers.GetProperty("type").GetString());
        Assert.Equal("integer", numbers.GetProperty("items").GetProperty("type").GetString());
    }
}
