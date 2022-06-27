using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface IFilterInterpreter
{
    IEnumerable<T> ApplyFilter<T>(IEnumerable<T> collection) where T : IPublishedElement;
}
public class DefaultFilterInterpreter : IFilterInterpreter
{
    protected readonly HttpContext httpContext;
    protected HttpRequest HttpRequest => httpContext.Request;
    protected string Culture => HttpRequest.Headers.ContentLanguage;
    protected IQueryCollection Query => HttpRequest.Query;
    protected readonly IPublishedUrlProvider publishedUrlProvider;

    public DefaultFilterInterpreter(
        IHttpContextAccessor httpContextAccessor,
        IPublishedUrlProvider publishedUrlProvider 
        )
    {
        this.httpContext = httpContextAccessor.HttpContext;
        this.publishedUrlProvider = publishedUrlProvider;
    }

    public virtual IEnumerable<T> ApplyFilter<T>(IEnumerable<T> collection) where T : IPublishedElement
    {
        if (!Query.TryGetValue("filter", out var filter))
            return collection;

        var filterSplit = filter.ToString().Split(',');
        foreach (var filterItem in filterSplit)
        {
            var split = filterItem.Split(' ');
            if (split.Length < 2)
                throw new InvalidFilterException(filterItem);

            var key = split[0];
            var opr = split[1];

            bool isNotOperator = opr.Equals("not", StringComparison.InvariantCultureIgnoreCase);
            if (isNotOperator)
            {
                opr = split[2];
                var val = String.Join(" ", split.Skip(3));
                collection = collection.Where(x => !ApplyOperator(x, key, opr, val));
            }
            else
            {
                var val = String.Join(" ", split.Skip(2));
                collection = collection.Where(x => ApplyOperator(x, key, opr, val));
            }

        }
        return collection;
    }

    public virtual bool ApplyOperator(IPublishedElement content, string propertyAlias, string opr, string input)
    {
        object propertyValue = GetPropertyValue(content, propertyAlias);
        return ApplyOperatorToValue(opr, input, propertyValue);
    }

    public virtual bool ApplyOperatorToValue(string opr, string input, object propertyValue)
    {
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

    public virtual object GetPropertyValue(IPublishedElement element, string propertyAlias)
    {
        var publishedProperty = element.GetProperty(propertyAlias);
        if (publishedProperty is not null)
            return publishedProperty.GetValue(Culture);

        var classProperty = element.GetType().GetProperties()
            .SingleOrDefault(x => x.Name.Equals(propertyAlias, StringComparison.InvariantCultureIgnoreCase));
        if (classProperty is not null)
            return classProperty.GetValue(element);

        switch (propertyAlias.ToLower())
        {
            case "url":
                if (element is IPublishedContent content)
                    return publishedUrlProvider.GetUrl(content, UrlMode.Relative, Culture);
                    // return content.Url(publishedUrlProvider, Culture, UrlMode.Relative);
                break;
        }

        throw new UnknownPropertyException(element.ContentType.Alias, propertyAlias);
    }
}