using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportGenerator.Domain.JsonConverters;

public class ObjectJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var dictionary = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dictionary;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var key = reader.GetString();
                    reader.Read();
                    var value = Read(ref reader, typeof(object), options);
                    dictionary[key] = value;
                }
            }
            return dictionary;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                var value = Read(ref reader, typeof(object), options);
                list.Add(value);
            }
            return list;
        }

        if (reader.TokenType == JsonTokenType.Number)
            return reader.TryGetInt64(out long l) ? l : reader.GetDouble();

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        throw new JsonException($"Unsupported JSON token: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}