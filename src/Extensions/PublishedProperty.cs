using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.Extensions;

public static class PublishedPropertyExtensions
{
    public static bool HasValueOfIEnumerableOfIPublishedContent(this IPublishedProperty property)
        => property.PropertyType.ClrType.IsEnumerableOfIPublishedContent()
        || property.PropertyType.ModelClrType.IsEnumerableOfIPublishedContent();
}