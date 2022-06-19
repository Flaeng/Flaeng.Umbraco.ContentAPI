using System.Collections.Generic;
using System.Text.Json.Serialization;
using Flaeng.Umbraco.ContentAPI.Converters;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(ObjectResponseConverter))]
public class ObjectResponse : PublishedContent
{
    [JsonPropertyName("_links")]
    public Dictionary<string, HalObject> Links { get; } = new Dictionary<string, HalObject>();

    [JsonPropertyName("_embedded")]
    public Dictionary<string, CollectionResponse> Embedded { get; set; }

    public ObjectResponse(IPublishedContent content)
        : base(content)
    {
    }
}