using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Models;

namespace Flaeng.Umbraco.ContentAPI.Converters;

public class HalObjectConverter : JsonConverter<HalObject>
{
    public override HalObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, HalObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in typeof(HalObject).GetProperties())
        {
            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                writer.WritePropertyName($"{Char.ToLower(prop.Name[0])}{prop.Name.Substring(1)}"); 
                JsonSerializer.Serialize(writer, propValue, options);
            }
        }
        writer.WriteEndObject();
    }
}