using System;

using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.Extensions;

public static class UmbracoContextAccessorExtensions
{
    public static IUmbracoContext GetUmbracoContext(this IUmbracoContextAccessor accessor)
        => accessor.TryGetUmbracoContext(out var res) ? res : default;

    public static IUmbracoContext GetUmbracoContextOrThrow(this IUmbracoContextAccessor accessor)
        => accessor.TryGetUmbracoContext(out var res) ? res : throw new Exception("Failed to obtain UmbracoContext");
}