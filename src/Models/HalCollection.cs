using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.JsonConverters;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(HalCollectionJsonConverter))]
public class HalCollection : ILinksContainer
{
    [JsonPropertyName("_links")]
    public Dictionary<string, LinkObject> Links { get; } = new();

    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalPageCount
    {
        get => TotalItemCount == 0 || PageSize == 0
            ? 0
            : (int)Math.Ceiling((decimal)TotalItemCount / PageSize);
    }
    public int TotalItemCount { get; init; }

    public IReadOnlyList<HalObject> Items { get; init; }

    [JsonIgnore]
    public string ItemContentType { get; init; }

    public HalCollection(
        string itemContentType, 
        int totalItemCount,
        IEnumerable<HalObject> items, 
        int pageNumber, 
        int pageSize)
    {
        this.ItemContentType = itemContentType;
        this.TotalItemCount = totalItemCount;
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Items = items.ToList();
    }
}