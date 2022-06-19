using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;

public interface IFilterHandler
{
    IEnumerable<IPublishedContent> ApplyFilter(IEnumerable<IPublishedContent> collection);
}
public class DefaultFilterHandler : IFilterHandler
{
    protected readonly HttpContext httpContext;
    protected HttpRequest HttpRequest => httpContext.Request;
    protected string Culture => HttpRequest.Headers.ContentLanguage;
    protected IQueryCollection Query => HttpRequest.Query;

    public DefaultFilterHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContext = httpContextAccessor.HttpContext;
    }

    public IEnumerable<IPublishedContent> ApplyFilter(IEnumerable<IPublishedContent> collection)
    {
        if (Query.TryGetValue("$filter", out var filter))
        {
            var filterSplit = filter.ToString().Split(',');
            foreach (var filterItem in filterSplit)
            {
                var split = filterItem.Split(' ');
                if (split.Length < 2)
                    throw new InvalidFilterException(filterItem);

                var key = split[0];
                var opr = split[1];
                var val = String.Join(" ", split.Skip(2));

                collection = collection.Where(x => ApplyOperator(x, key, opr, val));
            }
        }
        return collection;
    }

    public bool ApplyOperator(IPublishedContent content, string propertyAlias, string opr, string input)
    {
        var publishedProperty = content.GetProperty(propertyAlias);
        if (publishedProperty == null)
            throw new UnknownPropertyException(content.ContentType.Alias, propertyAlias);

        var propertyValue = publishedProperty.GetValue(Culture);

        if (propertyValue is IComparable propValue && propValue.GetType() != typeof(string))
        {
            var inputAsPropertyType = Convert.ChangeType(input, propertyValue.GetType());
            return opr switch
            {
                "eq" => propValue.Equals(inputAsPropertyType),
                "gt" => propValue.CompareTo(inputAsPropertyType) == 1,
                "lt" => propValue.CompareTo(inputAsPropertyType) == -1,
                "like" => propValue?.ToString()?.Contains(input, StringComparison.InvariantCultureIgnoreCase) ?? false,
                _ => throw new UnknownOperatorException(opr),
            };
        }
        var propertyValueString = propertyValue?.ToString() ?? "";
        return opr switch
        {
            "eq" => propertyValueString.Equals(input),
            "gt" => propertyValueString.CompareTo(input) == 1,
            "lt" => propertyValueString.CompareTo(input) == -1,
            "like" => propertyValueString?.Contains(input, StringComparison.InvariantCultureIgnoreCase) ?? false,
            _ => throw new UnknownOperatorException(opr),
        };
    }
}