using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.JsonConverters;
using Flaeng.Umbraco.Extensions;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(HalObjectJsonConverter))]
public class HalObject : ILinksContainer
{
    public Dictionary<string, LinkObject> Links { get; set; } = new();
    public Dictionary<string, HalCollection> Embedded { get; set; } = new();
    private Dictionary<HalProperty, object> properties { get; } = new();
    public IReadOnlyDictionary<HalProperty, object> Properties => properties;

    public int? Id { get; set; }
    public string Name { get; set; }
    public Guid Key { get; set; }
    public IPublishedContentType ContentType { get; set; }
    public IPublishedContent Parent { get; set; }

    public HalProperty GetProperty(string key) => Properties.Keys.SingleOrDefault(x => x.Alias == key);
    public object GetPropertyValue(string key) => Properties[GetProperty(key)];
    public void SetProperty<T>(string key, T value)
    {
        var existingKey = GetProperty(key);
        if (existingKey != null)
            properties.Remove(existingKey);

        var property = new HalProperty
        {
            Alias = key,
            ValueType = typeof(T)
        };
        properties.Add(property, value);
    }
}
public class HalProperty
{
    public string Alias { get; set; }
    public Type ValueType { get; set; }

    public bool IsValueTypeHalCollection()
        => ValueType == typeof(HalCollection) || ValueType.IsEnumerableOf<HalObject>();
}