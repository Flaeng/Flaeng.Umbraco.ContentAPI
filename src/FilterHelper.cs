using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;

public interface IFilterHelper
{
    IEnumerable<IPublishedContent> ApplyFilter(IEnumerable<IPublishedContent> collection);
}
public class FilterHelper : IFilterHelper
{
    protected readonly HttpContext httpContext;
    protected HttpRequest HttpRequest => httpContext.Request;
    protected IQueryCollection Query => HttpRequest.Query;

    public FilterHelper(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContext = httpContextAccessor.HttpContext;
    }

    public IEnumerable<IPublishedContent> ApplyFilter(IEnumerable<IPublishedContent> collection)
    {
        if (Query.TryGetValue("$filter", out var filter))
        {
            var filterSplit = filter.ToString().Split('&');
            foreach (var filterItem in filterSplit)
            {
                var split = filterItem.Split(' ');
                var key = split[0];
                var opr = split[1];
                var val = split[2];

                collection = collection.Where(x => ApplyOperator(x.GetProperty(key), opr, val));
            }
        }
        return collection;
    }

    private bool ApplyOperator(IPublishedProperty publishedProperty, string opr, string val)
    {
        return opr switch
        {
            "eq" => publishedProperty.GetValue(HttpRequest.Headers.ContentLanguage.ToString())?.ToString() == val,
            _ => throw new Exception($"Unknown operator '{opr}'"),
        };
    }
}