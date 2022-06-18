using System;
using System.Collections;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.Extensions;

public static class TypeExtensions
{
    public static bool IsEnumerableOfIPublishedContent(this Type t)
        => typeof(IEnumerable).IsAssignableFrom(t)
        && typeof(IPublishedContent).IsAssignableFrom(t.GetGenericArguments().FirstOrDefault());

}