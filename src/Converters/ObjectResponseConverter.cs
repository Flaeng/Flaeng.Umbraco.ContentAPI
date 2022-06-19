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
    protected readonly HttpContext httpContext;
    protected HttpRequest httpRequest { get => httpContext.Request; }
    protected string culture { get => httpRequest.Headers.ContentLanguage; }

    public ObjectResponseConverter()
    {
        this.httpContext = null; //httpContextAccessor.HttpContext;
    }

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

        foreach (var property in value.Properties)
        {
            writer.WritePropertyName(property.Alias);
            var propertyValue = property.GetValue();
            if (propertyValue is IEnumerable<IPublishedContent> coll)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else if (propertyValue is IPublishedContent content)
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else if (propertyValue is HtmlEncodedString hes)
                writer.WriteStringValue(hes.ToHtmlString());
            else
                JsonSerializer.Serialize(writer, propertyValue, options);
        }

        writer.WriteEndObject();
    }
}
