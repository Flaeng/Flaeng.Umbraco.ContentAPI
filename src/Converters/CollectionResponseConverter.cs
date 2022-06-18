using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flaeng.Umbraco.ContentAPI.Models;

namespace Flaeng.Umbraco.ContentAPI.Converters;

public class CollectionResponseConverter : JsonConverter<CollectionResponse>
{
    public override CollectionResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, CollectionResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in typeof(CollectionResponse).GetProperties())
        {
            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, propValue, options);
            }
        }
        writer.WriteEndObject();
    }
}
