using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.ContentAPI.Options;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface ILinkPopulator
{
    void PopulateRoot(LinksContainer item);
    void Populate(HalObject item, string[] expand);
    void Populate(HalCollection coll, string[] expand);
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

    public virtual void PopulateRoot(LinksContainer item)
    {
        if (options.HideLinks)
            return;

        AddSelfLink(item, new string[0]);
        AppendQueryStringToSelfLink(item);

        if (options.UmbracoOptions.ExposeMedia)
            item.Links.Add("media", new LinkObject { Name = "Media", Href = linkFormatter.FormatHref("media") });

        if (options.UmbracoOptions.ExposeMembers)
            item.Links.Add("members", new LinkObject { Name = "Members", Href = linkFormatter.FormatHref("members") });

        if (options.UmbracoOptions.ExposeMemberGroups)
            item.Links.Add("memberGroups", new LinkObject { Name = "Member Groups", Href = linkFormatter.FormatHref("memberGroups") });

        if (options.UmbracoOptions.ExposeUsers)
            item.Links.Add("users", new LinkObject { Name = "Users", Href = linkFormatter.FormatHref("users") });

        if (options.UmbracoOptions.ExposeForms)
            item.Links.Add("forms", new LinkObject { Name = "Forms", Href = linkFormatter.FormatHref("forms") });

        if (options.UmbracoOptions.ExposeTranslationDictionary)
            item.Links.Add("dictionary", new LinkObject { Name = "Dictionary", Href = linkFormatter.FormatHref("dictionary") });

        foreach (var contentType in contentTypeService.GetAll())
            item.Links.Add(contentType.Alias, new LinkObject { Name = contentType.Name, Href = linkFormatter.FormatHref(contentType.Alias) });
    }

    public virtual void AddSelfLink(ILinksContainer container, string[] expand)
    {
        switch (container)
        {
            case HalObject or:
                {
                    string path = or.Id != null
                        ? $"{or.ContentType.Alias}/{or.Id}"
                        // : $"{or.Parent.ContentType.Alias}/{or.Parent.Id}/{or.ContentType.Alias}/{or.Key}";
                        : $"{or.ContentType.Alias}/{or.Key}";

                    var hal = new LinkObject
                    {
                        Href = linkFormatter.FormatHref(path, expand)
                    };
                    container.Links.Add("self", hal);
                }
                break;

            case HalCollection cr:
                {
                    var hal = new LinkObject
                    {
                        Href = linkFormatter.FormatHref($"{cr.ItemContentType}", expand)
                    };
                    container.Links.Add("self", hal);
                }
                break;

            default:
                {
                    var hal = new LinkObject
                    {
                        Href = linkFormatter.FormatHref(String.Empty)
                    };
                    container.Links.Add("self", hal);
                }
                break;
        }
    }

    private void AppendQueryStringToSelfLink(ILinksContainer container)
    {
        container.Links["self"].Href += httpContext.Request.QueryString;
    }

    public virtual void Populate(HalObject item, string[] expand)
    //     => Populate(item, expand, false);

    // public virtual void Populate(HalObject item, string[] expand, bool isInternalCall)
    {
        if (options.HideLinks)
            return;

        AddSelfLink(item, expand);
        // if (isInternalCall == false)
        //     AppendQueryStringToSelfLink(item);

        if (item.Parent != null)
        {
            item.Links.Add("parent", new LinkObject
            {
                Name = item.Parent.Name,
                Href = linkFormatter.FormatHref($"{item.Parent.ContentType.Alias}/{item.Parent.Id}")
            });
        }

        var childContentTypeList = contentTypeService.GetChildren(item.ContentType.Id);
        foreach (var childContentType in childContentTypeList)
        {
            var alias = childContentType.Alias;
            item.Links.Add(alias, new LinkObject
            {
                Name = childContentType.Name,
                Href = linkFormatter.FormatHref($"{item.ContentType.Alias}/{item.Id}/{alias}")
            });
        }

        foreach (var property in item.Properties)
        {
            if (property.Value is not IEnumerable<HalObject>)
                continue;

            var alias = property.Key.Alias;
            item.Links.Add(alias, new LinkObject
            {
                Name = formatNameFromAlias(alias),
                Href = linkFormatter.FormatHref($"{item.ContentType.Alias}/{item.Id}/{alias}")
            });
        }
    }

    public virtual string formatNameFromAlias(string alias) => Char.ToUpper(alias[0]) + alias.Substring(1);

    public virtual void Populate(HalCollection coll, string[] expand)
    {
        AddSelfLink(coll, expand);
        // AppendQueryStringToSelfLink(coll);

        var currentUrl = httpContext.Request.Path.ToString();
        var currentQuery = new MutableQueryCollection(httpContext.Request.Query);
        if (coll.PageNumber > 1)
        {
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = "1";
                coll.Links.Add("first", new LinkObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = (coll.PageNumber - 1).ToString();
                coll.Links.Add("prev", new LinkObject { Href = $"{currentUrl}?{query}" });
            }
        }
        if (coll.PageNumber < coll.TotalPageCount)
        {
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = (coll.PageNumber + 1).ToString();
                coll.Links.Add("next", new LinkObject { Href = $"{currentUrl}?{query}" });
            }
            {
                var query = currentQuery.Copy();
                query["pageNumber"] = coll.TotalPageCount.ToString();
                coll.Links.Add("last", new LinkObject { Href = $"{currentUrl}?{query}" });
            }
        }

        // foreach (var item in coll.Items)
        // {
        //     // Populate(item, expand);
        //     // Populate(item, expand, true);

        //     var childHalObjects = item.Properties
        //         .Where(x => x.Value is IEnumerable<HalObject>)
        //         .Select(x => x.Value)
        //         .Cast<IEnumerable<HalObject>>()
        //         .SelectMany(x => x);
        //     foreach (var property in childHalObjects)
        //     {
        //         Populate(property, expand);
        //         // Populate(property, expand, true);
        //     }
        // }
    }
}