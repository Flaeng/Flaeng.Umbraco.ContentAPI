using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flaeng.Umbraco.ContentAPI.Models;

public class LinksObject : ILinksContainer
{
    [JsonPropertyName("_links")]
    public Dictionary<string, HalObject> Links { get; } = new Dictionary<string, HalObject>();
}