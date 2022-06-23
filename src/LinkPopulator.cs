using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.ContentAPI.Options;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI;

public interface ILinkPopulator
{
    LinksObject GetRoot();
    void Populate(ObjectResponse item);
    void Populate(CollectionResponse coll);
}
public class DefaultLinkPopulator : ILinkPopulator
{
    protected readonly HttpContext httpContext;
    protected readonly IUmbracoContext umbracoContext;
    protected readonly IContentTypeService contentTypeService;
    protected readonly IContentApiOptions options;
    protected readonly ILinkFormatter linkFormatter;

    public DefaultLinkPopulator(
        IHttpContextAccessor httpContextAccessor,
        IUmbracoContextAccessor umbracoContextAccessor,
        IContentTypeService contentTypeService,
        IOptionsSnapshot<ContentApiOptions> options,
        ILinkFormatter linkFormatter
    )
    {
        this.httpContext = httpContextAccessor.HttpContext;
        umbracoContextAccessor.TryGetUmbracoContext(out umbracoContext);
        this.contentTypeService = contentTypeService;
        this.options = options.Value;
        this.linkFormatter = linkFormatter;
    }

    public LinksObject GetRoot()
    {
        var resp = new LinksObject();
        if (options.HideLinks)
            return resp;

        AddSelfLink(resp);

        if (options.UmbracoOptions.ExposeMedia)
            resp.Links.Add("media", new HalObject { Name = "Media", Href = linkFormatter.FormatHref("media") });

        if (options.UmbracoOptions.ExposeMembers)
            resp.Links.Add("members", new HalObject { Name = "Members", Href = linkFormatter.FormatHref("members") });

        if (options.UmbracoOptions.ExposeMemberGroups)
            resp.Links.Add("memberGroups", new HalObject { Name = "Member Groups", Href = linkFormatter.FormatHref("memberGroups") });

        if (options.UmbracoOptions.ExposeUsers)
            resp.Links.Add("users", new HalObject { Name = "Users", Href = linkFormatter.FormatHref("users") });

        if (options.UmbracoOptions.ExposeForms)
            resp.Links.Add("forms", new HalObject { Name = "Forms", Href = linkFormatter.FormatHref("forms") });

        if (options.UmbracoOptions.ExposeTranslationDictionary)
            resp.Links.Add("dictionary", new HalObject { Name = "Dictionary", Href = linkFormatter.FormatHref("dictionary") });

        foreach (var contentType in contentTypeService.GetAll())
            resp.Links.Add(contentType.Alias, new HalObject { Name = contentType.Name, Href = linkFormatter.FormatHref(contentType.Alias) });

        return resp;
    }

    private void AddSelfLink(ILinksContainer links)
    {
        links.Links.Add("self", new HalObject { Href = httpContext.Request.Path });
    }

    public void Populate(ObjectResponse item)
    {
        if (options.HideLinks)
            return;

        AddSelfLink(item);

        if (item.Parent != null)
        {
            item.Links.Add("parent", new HalObject
            {
                Name = item.Parent.Name,
                Href = $"/{item.Parent.ContentType.Alias}/{item.Parent.Id}"
            });
        }

        var childContentTypeList = contentTypeService.GetChildren(item.ContentType.Id);
        foreach (var childContentType in childContentTypeList)
        {
            var alias = childContentType.Alias;
            item.Links.Add(alias, new HalObject
            {
                Name = childContentType.Name,
                Href = $"/{item.ContentType.Alias}/{item.Id}/{alias}"
            });
        }

        foreach (var property in item.Properties.Where(PropertyTypeIsEnumerableOfPublishedContent))
        {
            var alias = property.Alias;
            item.Links.Add(alias, new HalObject
            {
                Name = "",
                Href = $"/{item.ContentType.Alias}/{item.Id}/{alias}"
            });
        }
    }

    private static bool PropertyTypeIsEnumerableOfPublishedContent(IPublishedProperty prop)
    {
        return prop.PropertyType.ClrType.IsEnumerableOfIPublishedContent()
            || prop.PropertyType.ModelClrType.IsEnumerableOfIPublishedContent();
    }

    public void Populate(CollectionResponse coll)
    {
        AddSelfLink(coll);

        var currentUrl = httpContext.Request.Path.ToString();
        var currentQuery = new MutableQueryCollection(httpContext.Request.Query);
        if (coll.PageNumber != 1)
        {
            {
                var query = currentQuery.Copy();
                query["$pageNumber"] = "1";
                coll.Links.Add("first", new HalObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["$pageNumber"] = (coll.PageNumber - 1).ToString();
                coll.Links.Add("previous", new HalObject { Href = $"{currentUrl}?{query}" });
            }
        }
        if (coll.PageNumber != coll.TotalPageCount)
        {
            {
                var query = currentQuery.Copy();
                query["$pageNumber"] = (coll.PageNumber + 1).ToString();
                coll.Links.Add("next", new HalObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["$pageNumber"] = coll.TotalPageCount.ToString();
                coll.Links.Add("last", new HalObject { Href = $"{currentUrl}?{query}" });
            }
        }
    }
}