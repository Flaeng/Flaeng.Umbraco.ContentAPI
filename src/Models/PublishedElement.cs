using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Flaeng.Umbraco.Extensions;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Models;

public class PublishedElement : IPublishedElement
{
    [JsonIgnore] private IPublishedElement element { get; init; }
    [JsonIgnore] public IPublishedContentType ContentType => element.ContentType;
    public Guid Key => element.Key;
    [JsonIgnore] public List<IPublishedProperty> Properties { get; init; }
    [JsonIgnore] IEnumerable<IPublishedProperty> IPublishedElement.Properties => element.Properties;
    public Dictionary<string, IEnumerable<PublishedElement>> PropertyValues { get; } = new();

    public PublishedElement(IPublishedElement element, string culture)
    {
        this.element = element;
        this.Properties = element.Properties.ToList();

        foreach (var property in this.Properties.Where(x => x.HasValueOfIEnumerableOfIPublishedElement()))
        {
            var contentColl = property.GetValue(culture) as IEnumerable<IPublishedContent>;
            if (contentColl is not null)
            {
                this.PropertyValues[property.Alias] = contentColl
                    .Select(x => new ObjectResponse(x, culture))
                    .ToList();
                continue;
            }

            var elementColl = property.GetValue(culture) as IEnumerable<IPublishedElement>;
            if (elementColl is not null)
            {
                this.PropertyValues[property.Alias] = elementColl
                    .Select(x => new PublishedElement(x, culture))
                    .ToList();
                continue;
            }

        }
    }

    public IPublishedProperty GetProperty(string alias) => element.GetProperty(alias);
}