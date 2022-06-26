using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.JsonConverters;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(LinkObjectJsonConverter))]
public class LinkObject
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Href { get; set; }
    public bool? Templated { get; set; }
}