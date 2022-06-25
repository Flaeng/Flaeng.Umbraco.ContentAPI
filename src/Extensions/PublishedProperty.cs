using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.Extensions;

public static class PublishedPropertyExtensions
{
    public static bool HasValueOfIEnumerableOfIPublishedElement(this IPublishedProperty property)
        => property.PropertyType.ClrType.IsEnumerableOf<IPublishedElement>()
        || property.PropertyType.ModelClrType.IsEnumerableOf<IPublishedElement>();
}