using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Models;

using Umbraco.Cms.Core.Strings;

namespace Flaeng.Umbraco.ContentAPI.JsonConverters;

public class HalObjectJsonConverter : JsonConverter<HalObject>
{
    public override HalObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, HalObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Links != null && value.Links.Any())
        {
            writer.WritePropertyName("_links");
            JsonSerializer.Serialize(writer, value.Links, options);
        }

        writeProperties(writer, value, options);

        if (value.Embedded != null && value.Embedded.Any())
        {
            writer.WritePropertyName("_embedded");
            JsonSerializer.Serialize(writer, value.Embedded, options);
        }

        writer.WriteEndObject();
    }

    private void writeProperties(Utf8JsonWriter writer, HalObject value, JsonSerializerOptions options)
    {
        foreach (var property in value.Properties)
        {
            if (property.Value == null)
            {
                writer.WriteNull(property.Key.Alias);
                continue;
            }

            if (property.Key.IsValueTypeHalCollection())
            {
                var colleciton = property.Value as IEnumerable<HalObject>;
                writeCollectionProperty(writer, property.Key.Alias, colleciton, options);
            }
            else 
                writeSimpleProperty(writer, property.Key.Alias, property.Value, options);
        }
    }

    private void writeSimpleProperty(
        Utf8JsonWriter writer,
        string alias, 
        object value, 
        JsonSerializerOptions options 
        )
    {
        switch (value)
        {
            case HtmlEncodedString htmlEncodedString:
                string html = htmlEncodedString.ToHtmlString();
                writer.WriteString(alias, html);
                break;

            // case HalCollection halCollection:
            //     Write(writer, objectResponse, options);
            //     break;

            case IEnumerable<HalObject> collection:
                writer.WritePropertyName(alias);
                writer.WriteStartArray();
                foreach (var item in collection)
                    Write(writer, item, options);
                writer.WriteEndArray();
                break;

            case string str:
                writer.WriteString(alias, str);
                break;

            default:
                writer.WritePropertyName(alias);
                // writer.WriteString(property.Alias, $"2Type: ({propertyValue.GetType()})");
                // writer.WriteString(alias, "TODO:" + value.ToString());
                JsonSerializer.Serialize(writer, value, options);
                break;

                // default:
                //     break;
        }
    }

    private void writeCollectionProperty(
        Utf8JsonWriter writer,
        string key,
        IEnumerable<HalObject> value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(key);
        writer.WriteStartArray();
        foreach (var item in value)
            Write(writer, item, options);
        writer.WriteEndArray();
    }
}