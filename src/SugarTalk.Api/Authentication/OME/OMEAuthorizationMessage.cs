using Newtonsoft.Json;

namespace SugarTalk.Api.Authentication.OME;

public class OMEAuthorizationMessage
{
    public bool Active { get; set; }
    
    public string Username { get; set; }
    
    public string Sub { get; set; }
    
    public string Nickname { get; set; }
    
    public string CreatedWay { get; set; }
    
    [JsonConverter(typeof(AudConverter))]
    public List<string> Aud { get; set; }
    
    public int Exp { get; set; }
}

public class AudConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(string) || objectType == typeof(List<string>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return reader.TokenType switch
        {
            JsonToken.StartArray => serializer.Deserialize<List<string>>(reader) ?? new List<string>(),
            JsonToken.String => new List<string> { serializer.Deserialize<string>(reader) ?? string.Empty },
            JsonToken.Null => new List<string>(),
            _ => throw new JsonSerializationException("Unexpected token type for Aud property.")
        };
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        switch (value)
        {
            case List<string> list:
                serializer.Serialize(writer, list);
                break;
            case string str:
                serializer.Serialize(writer, str);
                break;
            default:
                throw new JsonSerializationException("Unexpected data type for Aud property.");
        }
    }
}