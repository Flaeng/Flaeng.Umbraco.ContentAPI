using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Models;

namespace Flaeng.Umbraco.ContentAPI.JsonConverters;

public class LinkObjectJsonConverter : JsonConverter<LinkObject>
{
    public override LinkObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, LinkObject value, JsonSerializerOptions options)
    {
        var halObjectProperties = typeof(LinkObject).GetProperties();
        if (halObjectProperties.All(x => x.GetValue(value) == null || x.Name == "Href"))
        {
            writer.WriteStringValue(value.Href);
            return;
        }

        writer.WriteStartObject();
        foreach (var prop in halObjectProperties)
        {
            var isIgnore = prop.GetCustomAttributes<JsonIgnoreAttribute>();
            if (isIgnore.Count() != 0)
                continue;

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