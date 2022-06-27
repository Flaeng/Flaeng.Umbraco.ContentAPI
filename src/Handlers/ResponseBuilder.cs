using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Flaeng.Umbraco.ContentAPI.Handlers;
public interface IResponseBuilder
{
    LinksContainer Build(RootInterpreterResult request);
    HalObject Build(ObjectInterpreterResult request);
    HalCollection Build(CollectionInterpreterResult request);
}
public class DefaultResponseBuilder : IResponseBuilder
{
    protected readonly IUmbracoContext umbracoContext;
    protected readonly HttpContext httpContext;
    protected HttpRequest Request => httpContext.Request;
    protected string Culture => Request.Headers.ContentLanguage;
    protected readonly ILinkPopulator linkPopulator;
    protected readonly IPublishedUrlProvider publishedUrlProvider;

    public DefaultResponseBuilder(
        IUmbracoContextAccessor umbracoContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ILinkPopulator linkPopulator,
        IPublishedUrlProvider publishedUrlProvider
        )
    {
        this.umbracoContext = umbracoContextAccessor.GetUmbracoContextOrThrow();
        this.httpContext = httpContextAccessor.HttpContext;
        this.linkPopulator = linkPopulator;
        this.publishedUrlProvider = publishedUrlProvider;
    }

    public virtual LinksContainer Build(RootInterpreterResult request)
    {
        var result = new LinksContainer();
        linkPopulator.PopulateRoot(result);
        return result;
    }

    public virtual HalObject Build(ObjectInterpreterResult request)
        => ConvertToHalObject(request.Value);

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

    public HalObject ConvertToHalObject(IPublishedElement element)
    {
        string expandParam = ((string)httpContext.Request.Query["expand"]) ?? String.Empty;
        string[] expandSplit = ConvertExpandStringToArray(expandParam);
        return RecursiveConvert(element, expandSplit);
    }

    protected virtual HalObject RecursiveConvert(IPublishedElement element, string[] expandPath)
    {
        var result = new HalObject();
        result.Key = element.Key;
        result.ContentType = element.ContentType;

        if (element is IPublishedContent content)
        {
            result.Id = content.Id;
            result.Name = content.Name;
            result.Parent = content.Parent;
            result.SetProperty("id", content.Id);
            result.SetProperty("key", content.Key);
            result.SetProperty("name", content.Name);
            result.SetProperty("level", content.Level);
            result.SetProperty("createDate", content.CreateDate);
            result.SetProperty("creatorId", content.CreatorId);
            result.SetProperty("updateDate", content.UpdateDate);
            result.SetProperty("writerId", content.WriterId);
            result.SetProperty("parentId", content.Parent?.Id ?? -1);
            result.SetProperty("path", content.Path);
            result.SetProperty("sortOrder", content.SortOrder);
            result.SetProperty("url", publishedUrlProvider.GetUrl(content, UrlMode.Relative, Culture));
        }

        Expand(element, expandPath, result);
        linkPopulator.Populate(result, expandPath, element);
        return result;
    }

    private void Expand(IPublishedElement element, string[] expandPath, HalObject result)
    {
        var propertiesToExpand = expandPath.Where(x => !x.Contains(".")).ToList();
        foreach (var property in element.Properties)
        {
            var isElementValue = property.HasValueOfIPublishedElement();
            var isEnumerableValue = property.HasValueOfIEnumerableOfIPublishedElement();

            if (isElementValue == false && isEnumerableValue == false)
            {
                result.SetProperty(property.Alias, property.GetValue(Culture));
                continue;
            }

            if (!expandPath.Contains(property.Alias))
                continue;
            propertiesToExpand.Remove(property.Alias);

            object value = isElementValue
                ? handleElementPropertyValue(property, expandPath)
                : handleEnumeablePropertyValue(property, expandPath);

            result.SetProperty(property.Alias, value);
        }
        if (propertiesToExpand.Any())
            throw new ExpansionException(propertiesToExpand.First());
    }

    private HalObject handleElementPropertyValue(IPublishedProperty property, string[] expandPath)
    {
        var elementValue = property.GetValue(Culture) as IPublishedElement;
        return ConvertToHalObject(elementValue);
    }

    private IEnumerable<HalObject> handleEnumeablePropertyValue(IPublishedProperty property, string[] expandPath)
    {
        var list = property.GetValue(Culture) as IEnumerable<IPublishedElement>
            ?? new IPublishedElement[0];

        var childExpand = expandPath
            .Where(x => x.StartsWith($"{property.Alias}."))
            .Select(x => x.Substring(x.IndexOf('.') + 1))
            .Where(x => !String.IsNullOrWhiteSpace(x))
            .ToArray();

        var result = new List<HalObject>();
        foreach (var item in list)
            result.Add(RecursiveConvert(item, childExpand));
        return result;
    }

    public virtual HalCollection Build(CollectionInterpreterResult request)
    {
        var collection = request.Value;

        int pageNumber, pageSize;

        if (int.TryParse(Request.Query.GetValue("pageNumber", "1"), out pageNumber) == false)
            throw new InvalidQueryParameterException("pageNumber");

        if (int.TryParse(Request.Query.GetValue("pageSize", HalCollection.DefaultPageSize.ToString()), out pageSize) == false)
            throw new InvalidQueryParameterException("pageSize");

        string orderBy = Request.Query.GetValue("orderBy");
        if (String.IsNullOrWhiteSpace(orderBy) == false)
        {
            var contentType = umbracoContext.Content.GetContentType(request.ContentTypeAlias);
            if (contentType.PropertyTypes.Any(x => x.Alias == orderBy) == false)
                throw new InvalidQueryParameterException("orderBy");

            collection = collection.OrderBy(x => x.GetProperty(orderBy).GetValue(Culture));
        }

        var totalItemCount = collection.Count();
        var page = collection
            .Skip(pageNumber * pageSize - pageSize)
            .Take(pageSize)
            .Select(x => ConvertToHalObject(x))
            .ToArray();
        var result = new HalCollection(request.ContentTypeAlias, totalItemCount, page, pageNumber, pageSize);

        string expandParam = ((string)httpContext.Request.Query["expand"]) ?? String.Empty;
        string[] expand = ConvertExpandStringToArray(expandParam);
        linkPopulator.Populate(result, expand);

        return result;
    }

}