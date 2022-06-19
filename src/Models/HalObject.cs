using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Flaeng.Umbraco.ContentAPI.Converters;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(HalObjectConverter))]
public class HalObject
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Href { get; set; }
    public bool? Templated { get; set; }
}
public class LinksObject
{
    [JsonPropertyName("_links")]
    public Dictionary<string, HalObject> Links { get; set; }
}