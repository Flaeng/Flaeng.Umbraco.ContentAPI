using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface IExpander
{
    void Expand(PublishedElement content);
}
public static class ExpanderExtensions
{
    public static void Expand(this IExpander expander, CollectionResponse collection)
    {
        foreach (var item in collection.Items)
            expander.Expand(item);
    }
}
public class DefaultExpander : IExpander
{
    protected readonly ILogger<DefaultExpander> logger;
    protected readonly HttpContext httpContext;

    public DefaultExpander(
        ILogger<DefaultExpander> logger,
        IHttpContextAccessor httpContextAccessor
    )
    {
        this.logger = logger;
        this.httpContext = httpContextAccessor.HttpContext;
    }

    public virtual void Expand(PublishedElement content)
    {
        string expand = ((string)httpContext.Request.Query["expand"]) ?? String.Empty;
        string[] expandSplit = ConvertExpandStringToArray(expand);

        Expand(content, expandSplit);
    }

    public virtual string[] ConvertExpandStringToArray(string expand)
    {
        if (String.IsNullOrWhiteSpace(expand))
            return new string[0];

        return expand.Split(',').SelectMany(x =>
        {
            var splitted = x.Split('.');
            return Enumerable.Range(1, splitted.Length)
                .Select(x => String.Join(".", splitted.Take(x)));
        }).ToArray();
    }

    public virtual void Expand(PublishedElement content, string[] expand)
    {
        var propertiesToExpand = expand.Where(x => !x.Contains(".")).ToList();
        foreach (var prop in content.Properties.Where(x => x.HasValueOfIEnumerableOfIPublishedElement()))
        {
            if (!expand.Contains(prop.Alias))
            {
                content.PropertyValues[prop.Alias] = null;
                continue;
            }

            propertiesToExpand.Remove(prop.Alias);

            if (!content.PropertyValues.TryGetValue(prop.Alias, out var list) || list == null)
                continue;

            var childExpand = expand
                .Where(x => x.StartsWith($"{prop.Alias}."))
                .Select(x => x.Substring(x.IndexOf('.') + 1))
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ToArray();

            foreach (var item in list)
                Expand(item, childExpand);
        }
        if (propertiesToExpand.Any())
            throw new ExpansionException(propertiesToExpand.First());
    }

}