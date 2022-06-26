using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flaeng.Umbraco.ContentAPI.Models;

public interface ILinksContainer
{
    Dictionary<string, LinkObject> Links { get; }
}
public class LinksContainer : ILinksContainer
{
    [JsonPropertyName("_links")]
    public Dictionary<string, LinkObject> Links { get; } = new Dictionary<string, LinkObject>();
}