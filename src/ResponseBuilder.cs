using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;
public interface IResponseBuilder
{
    ObjectResponse Build(IPublishedContent content);
    CollectionResponse Build(string contentType, IEnumerable<IPublishedContent> contentColl);
}
public class DefaultResponseBuilder : IResponseBuilder
{
    protected readonly HttpContext httpContext;
    protected HttpRequest Request => httpContext.Request;
    protected readonly ILinkPopulator linkPopulator;

    public DefaultResponseBuilder(
        IHttpContextAccessor httpContextAccessor,
        ILinkPopulator linkPopulator
        )
    {
        this.httpContext = httpContextAccessor.HttpContext;
        this.linkPopulator = linkPopulator;
    }

    public ObjectResponse Build(IPublishedContent content)
    {
        var result = new ObjectResponse(content);
        expandObject(result);
        return result;
    }

    private void expandObject(ObjectResponse result)
    {
        linkPopulator.Populate(result);

        if (!httpContext.Request.Query.TryGetValue("expand", out var expand))
            expand = "";

        var expandSplit = expand.ToString().Split(',').SelectMany(x =>
        {
            var splitted = x.Split('.');
            return Enumerable.Range(1, splitted.Length + 1)
                .Select(x => String.Join(".", splitted.Take(x)));
        }).ToArray();

        expandObject(result, expandSplit);
    }

    private void expandObject(ObjectResponse result, string[] expand)
    {
        foreach (var prop in result.Properties.Where(x => x.HasValueOfIEnumerableOfIPublishedContent()))
        {
            if (!expand.Contains(prop.Alias))
            {
                result.PropertyValues[prop.Alias] = null;
            }
            else
            {
                var propValue = prop.GetValue() as IEnumerable<IPublishedContent>;
                if (propValue == null)
                    continue;

                var childExpand = expand.Where(x => x.StartsWith($"{prop.Alias}.")).ToArray();
                foreach (var item in propValue)
                    expandObject(new ObjectResponse(item), childExpand);
            }
        }
    }

    public CollectionResponse Build(string contentType, IEnumerable<IPublishedContent> contentColl)
    {
        var pageNumber = int.Parse(GetFromQuery("pageNumber", "1"));
        var pageSize = int.Parse(GetFromQuery("pageSize", "20"));
        if (Request.Query.TryGetValue("orderBy", out var orderBy))
            contentColl = contentColl.OrderBy(x => x.GetProperty(orderBy));

        var result = new CollectionResponse(contentType, contentColl, pageNumber, pageSize);
        linkPopulator.Populate(result);
        foreach (var item in result.Items)
            expandObject(item);
        return result;
    }

    private string GetFromQuery(string key, string defaultValue)
            => Request.Query.TryGetValue(key, out var value) ? value : defaultValue;

}