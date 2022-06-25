using System;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.ContentAPI.Options;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface ILinkPopulator
{
    void PopulateRoot(LinksObject item);
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
        IOptions<ContentApiOptions> options,
        ILinkFormatter linkFormatter
    )
    {
        this.httpContext = httpContextAccessor.HttpContext;
        this.umbracoContext = umbracoContextAccessor.GetUmbracoContextOrThrow();
        this.contentTypeService = contentTypeService;
        this.options = options.Value;
        this.linkFormatter = linkFormatter;
    }

    public virtual void PopulateRoot(LinksObject item)
    {
        if (options.HideLinks)
            return;

        AddSelfLink(item);

        if (options.UmbracoOptions.ExposeMedia)
            item.Links.Add("media", new HalObject { Name = "Media", Href = linkFormatter.FormatHref("media") });

        if (options.UmbracoOptions.ExposeMembers)
            item.Links.Add("members", new HalObject { Name = "Members", Href = linkFormatter.FormatHref("members") });

        if (options.UmbracoOptions.ExposeMemberGroups)
            item.Links.Add("memberGroups", new HalObject { Name = "Member Groups", Href = linkFormatter.FormatHref("memberGroups") });

        if (options.UmbracoOptions.ExposeUsers)
            item.Links.Add("users", new HalObject { Name = "Users", Href = linkFormatter.FormatHref("users") });

        if (options.UmbracoOptions.ExposeForms)
            item.Links.Add("forms", new HalObject { Name = "Forms", Href = linkFormatter.FormatHref("forms") });

        if (options.UmbracoOptions.ExposeTranslationDictionary)
            item.Links.Add("dictionary", new HalObject { Name = "Dictionary", Href = linkFormatter.FormatHref("dictionary") });

        foreach (var contentType in contentTypeService.GetAll())
            item.Links.Add(contentType.Alias, new HalObject { Name = contentType.Name, Href = linkFormatter.FormatHref(contentType.Alias) });
    }

    public virtual void AddSelfLink(ILinksContainer container)
    {
        if (container is ObjectResponse or)
            container.Links.Add("self", new HalObject { Href = linkFormatter.FormatHref($"{or.ContentType.Alias}/{or.Id}") });
        else if (container is CollectionResponse cr)
            container.Links.Add("self", new HalObject { Href = linkFormatter.FormatHref($"{cr.ItemContentType}") });
        else
            container.Links.Add("self", new HalObject { Href = linkFormatter.FormatHref(String.Empty) });

        container.Links["self"].Href += httpContext.Request.QueryString;
    }

    public virtual void Populate(ObjectResponse item)
    {
        if (options.HideLinks)
            return;

        AddSelfLink(item);

        if (item.Parent != null)
        {
            item.Links.Add("parent", new HalObject
            {
                Name = item.Parent.Name,
                Href = linkFormatter.FormatHref($"{item.Parent.ContentType.Alias}/{item.Parent.Id}")
            });
        }

        var childContentTypeList = contentTypeService.GetChildren(item.ContentType.Id);
        foreach (var childContentType in childContentTypeList)
        {
            var alias = childContentType.Alias;
            item.Links.Add(alias, new HalObject
            {
                Name = childContentType.Name,
                Href = linkFormatter.FormatHref($"{item.ContentType.Alias}/{item.Id}/{alias}")
            });
        }

        foreach (var property in item.Properties.Where(x => x.HasValueOfIEnumerableOfIPublishedElement()))
        {
            var alias = property.Alias;
            item.Links.Add(alias, new HalObject
            {
                Name = formatNameFromAlias(property.Alias),
                Href = linkFormatter.FormatHref($"{item.ContentType.Alias}/{item.Id}/{alias}")
            });
        }
    }

    public virtual string formatNameFromAlias(string alias) => Char.ToUpper(alias[0]) + alias.Substring(1);

    public virtual void Populate(CollectionResponse coll)
    {
        AddSelfLink(coll);

        var currentUrl = httpContext.Request.Path.ToString();
        var currentQuery = new MutableQueryCollection(httpContext.Request.Query);
        if (coll.PageNumber > 1)
        {
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = "1";
                coll.Links.Add("first", new HalObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = (coll.PageNumber - 1).ToString();
                coll.Links.Add("prev", new HalObject { Href = $"{currentUrl}?{query}" });
            }
        }
        if (coll.PageNumber < coll.TotalPageCount)
        {
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = (coll.PageNumber + 1).ToString();
                coll.Links.Add("next", new HalObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = coll.TotalPageCount.ToString();
                coll.Links.Add("last", new HalObject { Href = $"{currentUrl}?{query}" });
            }
        }

        foreach (var item in coll.Items)
        {
            Populate(item);
            foreach (var property in item.PropertiesWithIPublishedContentValue.SelectMany(x => x))
            {
                Populate(property);
            }
        }
    }
}