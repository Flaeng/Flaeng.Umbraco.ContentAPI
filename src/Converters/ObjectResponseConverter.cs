using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Models;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;

namespace Flaeng.Umbraco.ContentAPI.Converters;
public class ObjectResponseConverter : JsonConverter<ObjectResponse>
{
    public override ObjectResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ObjectResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Links != null && value.Links.Any())
        {
            writer.WritePropertyName("_links");
            JsonSerializer.Serialize(writer, value.Links, options);
        }

        writer.WriteNumber("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteNumber("level", value.Level);
        writer.WriteString("contentType", value.ContentType.Alias);
        writer.WriteString("createDate", value.CreateDate);
        writer.WriteNumber("creatorId", value.CreatorId);
        writer.WriteString("updateDate", value.UpdateDate);
        writer.WriteNumber("writerId", value.WriterId);

        writer.WriteString("key", value.Key);

        writer.WriteNumber("parentId", value.Parent?.Id ?? -1);

        writer.WriteString("path", value.Path);
        writer.WriteNumber("sortOrder", value.SortOrder);

        writeProperties(writer, value, options);

        writer.WriteEndObject();
    }

    public void WriteElement(Utf8JsonWriter writer, PublishedElement value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writeProperties(writer, value, options);
        writer.WriteEndObject();
    }

    private void writeProperties(Utf8JsonWriter writer, PublishedElement value, JsonSerializerOptions options)
    {
        foreach (var property in value.Properties)
        {
            var childOptions = new JsonSerializerOptions(options);
            childOptions.MaxDepth = 1;

            // if PropertyValue is of type IEnumerable<ObjectResponse>
            if (value.PropertyValues.TryGetValue(property.Alias, out var collection))
            {
                // if this property is not expanded
                if (collection == null)
                    continue;
                writeCollectionProperty(writer, collection, childOptions, property);
            }
            else writeSimpleProperty(writer, value, childOptions, property);
        }
    }

    private void writeSimpleProperty(Utf8JsonWriter writer, PublishedElement value, JsonSerializerOptions options, IPublishedProperty property)
    {
        var propertyValue = value.GetProperty(property.Alias).GetValue();
        switch (propertyValue)
        {
            case HtmlEncodedString htmlEncodedString:
                string html = htmlEncodedString.ToHtmlString();
                writer.WriteString(property.Alias, html);
                break;

            case ObjectResponse objectResponse:
                Write(writer, objectResponse, options);
                break;

            case IEnumerable<PublishedElement> publishedContent:
                writer.WriteString(property.Alias, "(TODO3)");
                break;

            case string str:
                writer.WriteString(property.Alias, str);
                break;

            default:
                // writer.WriteString(property.Alias, $"2Type: ({propertyValue.GetType()})");
                writer.WriteString(property.Alias, "TODO:" + propertyValue?.ToString());
                
                // JsonSerializer.Serialize(writer, propertyValue, options);
                break;

            // default:
            //     break;
        }
    }

    private void writeCollectionProperty(
        Utf8JsonWriter writer,
        IEnumerable<PublishedElement> value,
        JsonSerializerOptions options,
        IPublishedProperty property)
    {
        writer.WritePropertyName(property.Alias);
        writer.WriteStartArray();
        foreach (var item in value)
            WriteElement(writer, item, options);
        writer.WriteEndArray();
    }
}