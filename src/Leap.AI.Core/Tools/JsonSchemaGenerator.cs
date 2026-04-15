using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leap.AI.Core.Tools;

/// <summary>
/// Generates a basic JSON Schema from a .NET type using reflection.
/// Supports: primitives, strings, enums, nullable types, arrays, List&lt;T&gt;, and nested objects.
/// </summary>
public static class JsonSchemaGenerator
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonElement Generate<T>() => Generate(typeof(T));

    public static JsonElement Generate(Type type)
    {
        var schema = BuildSchema(type, []);
        return JsonSerializer.SerializeToElement(schema, _opts);
    }

    private static Dictionary<string, object?> BuildSchema(Type type, HashSet<Type> visited)
    {
        // Unwrap Nullable<T>
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string))   return Primitive("string");
        if (type == typeof(bool))     return Primitive("boolean");
        if (type == typeof(Guid))     return new() { ["type"] = "string", ["format"] = "uuid" };
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return new() { ["type"] = "string", ["format"] = "date-time" };

        if (type == typeof(int) || type == typeof(long) ||
            type == typeof(short)   || type == typeof(byte))
            return Primitive("integer");

        if (type == typeof(float)   || type == typeof(double) || type == typeof(decimal))
            return Primitive("number");

        if (type.IsEnum)
            return new() { ["type"] = "string", ["enum"] = Enum.GetNames(type) };

        // Arrays and List<T>
        Type? elementType = null;
        if (type.IsArray)
            elementType = type.GetElementType();
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            elementType = type.GetGenericArguments()[0];

        if (elementType is not null)
            return new() { ["type"] = "array", ["items"] = BuildSchema(elementType, visited) };

        // Circular reference guard
        if (visited.Contains(type))
            return Primitive("object");

        visited.Add(type);
        var properties = new Dictionary<string, object?>();
        var required = new List<string>();
        var ctx = new NullabilityInfoContext();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            var jsonName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                ?? JsonNamingPolicy.CamelCase.ConvertName(prop.Name);

            properties[jsonName] = BuildSchema(prop.PropertyType, new HashSet<Type>(visited));

            var nullInfo = ctx.Create(prop);
            bool isNullable = !prop.PropertyType.IsValueType
                && nullInfo.WriteState == NullabilityState.Nullable;
            if (!isNullable) required.Add(jsonName);
        }

        visited.Remove(type);

        var schema = new Dictionary<string, object?> { ["type"] = "object", ["properties"] = properties };
        if (required.Count > 0) schema["required"] = required;
        return schema;
    }

    private static Dictionary<string, object?> Primitive(string type) => new() { ["type"] = type };
}
