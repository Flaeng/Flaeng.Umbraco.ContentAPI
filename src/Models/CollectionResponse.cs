using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.ContentAPI.Converters;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

[JsonConverter(typeof(CollectionResponseConverter))]
public class CollectionResponse
{
    [JsonPropertyName("_links")]
    public Dictionary<string, HalObject> Links { get; set; }

    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalPageCount
    {
        get => TotalItemCount == 0 || PageSize == 0
            ? 0
            : (int)Math.Ceiling((decimal)TotalItemCount / PageSize);
    }
    public int TotalItemCount { get; init; }

    public IEnumerable<ObjectResponse> Items { get; init; }

    public CollectionResponse(IEnumerable<IPublishedContent> items, int pageNumber, int pageSize)
    {
        this.TotalItemCount = items.Count();
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Items = items
            .Skip(PageNumber * PageSize - pageSize)
            .Take(PageSize)
            .Select(x => new ObjectResponse(x))
            .ToArray();
    }
}