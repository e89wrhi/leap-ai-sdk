using AiSdk.Entities.Common;
using AiSdk.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiSdk.Entities.Base;

public abstract class AiEntity : IAiEntity
{
    [JsonIgnore]
    public AiResponse? AiResponse { get; set; }

    private string? _rawJson;
    private JsonElement? _rawJsonElement;

    [JsonIgnore]
    public JsonElement? RawJsonElement
    {
        get
        {
            if (_rawJsonElement == null && _rawJson != null)
            {
                _rawJsonElement = JsonDocument.Parse(_rawJson).RootElement;
            }
            return _rawJsonElement;
        }
        internal set => _rawJsonElement = value;
    }

    internal void SetRawJson(string json)
    {
        _rawJson = json;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions { WriteIndented = true });
    }
}
