using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Models;

namespace Flaeng.Umbraco.ContentAPI.JsonConverters;

public class HalCollectionJsonConverter : JsonConverter<HalCollection>
{
    public override HalCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, HalCollection value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in typeof(HalCollection).GetProperties())
        {
            var isIgnore = prop.GetCustomAttributes<JsonIgnoreAttribute>();
            if (isIgnore.Count() != 0)
                continue;

            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                switch (prop.Name)
                {
                    case "Links":
                        writer.WritePropertyName("_links");
                        break;
                    case "Embedded":
                        writer.WritePropertyName("_embedded");
                        break;
                    default:
                        writer.WritePropertyName($"{Char.ToLower(prop.Name[0])}{prop.Name.Substring(1)}");
                        break;
                }
                JsonSerializer.Serialize(writer, propValue, options);
            }
        }
        writer.WriteEndObject();
    }
}