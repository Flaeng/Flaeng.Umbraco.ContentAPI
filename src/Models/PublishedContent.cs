using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

public class PublishedContent : IPublishedContent
{
    private IPublishedContent Content { get; init; }
    public int Id => Content.Id;
    public string Name => Content.Name;
    public string UrlSegment => Content.UrlSegment;
    public int SortOrder => Content.SortOrder;
    public int Level => Content.Level;
    public string Path => Content.Path;
    public int? TemplateId => Content.TemplateId;
    public int CreatorId => Content.CreatorId;
    public DateTime CreateDate => Content.CreateDate;
    public int WriterId => Content.WriterId;
    public DateTime UpdateDate => Content.UpdateDate;
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => Content.Cultures;
    public PublishedItemType ItemType => Content.ItemType;
    public IPublishedContent Parent => Content.Parent;
    public IEnumerable<IPublishedContent> Children => Content.Children;
    public IEnumerable<IPublishedContent> ChildrenForAllCultures => Content.ChildrenForAllCultures;
    public IPublishedContentType ContentType => Content.ContentType;
    public Guid Key => Content.Key;
    public Dictionary<string, object> PropertyValues { get; } = new();
    public List<IPublishedProperty> Properties { get; }
    IEnumerable<IPublishedProperty> IPublishedElement.Properties => this.Properties;
    public IPublishedProperty GetProperty(string alias) => Content.GetProperty(alias);
    public bool IsDraft(string culture = null) => Content.IsDraft(culture);
    public bool IsPublished(string culture = null) => Content.IsPublished(culture);

    public PublishedContent(IPublishedContent content)
    {
        this.Content = content;
        this.Properties = content.Properties.ToList();
    }
}
