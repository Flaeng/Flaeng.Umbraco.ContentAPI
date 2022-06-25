using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.Extensions;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

public class PublishedContent : PublishedElement, IPublishedContent
{
    [JsonIgnore] private IPublishedContent Content { get; init; }
    public int Id => Content.Id;
    public string Name => Content.Name;
    [JsonIgnore] public string UrlSegment => Content.UrlSegment;
    public int SortOrder => Content.SortOrder;
    public int Level => Content.Level;
    [JsonIgnore] public string Path => Content.Path;
    [JsonIgnore] public int? TemplateId => Content.TemplateId;
    public int CreatorId => Content.CreatorId;
    public DateTime CreateDate => Content.CreateDate;
    public int WriterId => Content.WriterId;
    public DateTime UpdateDate => Content.UpdateDate;
    [JsonIgnore] public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => Content.Cultures;
    [JsonIgnore] public PublishedItemType ItemType => Content.ItemType;
    public IPublishedContent Parent => Content.Parent;
    public IEnumerable<IPublishedContent> Children => Content.Children;
    [JsonIgnore] public IEnumerable<IPublishedContent> ChildrenForAllCultures => Content.ChildrenForAllCultures;
    [JsonIgnore] public IEnumerable<IEnumerable<ObjectResponse>> PropertiesWithIPublishedContentValue => Properties
        .Where(x => x.HasValueOfIEnumerableOfIPublishedElement())
        .Select(x => PropertyValues.TryGetValue(x.Alias, out var value) ? value as IEnumerable<ObjectResponse> : null)
        .Where(x => x != null);
    [JsonIgnore] IEnumerable<IPublishedProperty> IPublishedElement.Properties => this.Properties;

    public PublishedContent(IPublishedContent content, string culture)
        : base(content, culture)
    {
        this.Content = content;
    }

    public bool IsDraft(string culture = null)
        => Content.IsDraft(culture);

    public bool IsPublished(string culture = null)
        => Content.IsPublished(culture);
}