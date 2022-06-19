using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI;

public interface ILinkPopulator
{
    LinksObject GetRoot();
    void Populate(ObjectResponse item);
}
public class DefaultLinkPopulator : ILinkPopulator
{
    protected readonly IUmbracoContext umbracoContext;
    protected readonly IContentTypeService contentTypeService;
    protected readonly ContentApiOptions options;

    public DefaultLinkPopulator(
        IUmbracoContextAccessor umbracoContextAccessor,
        IContentTypeService contentTypeService,
        IOptions<ContentApiOptions> options
    )
    {
        umbracoContextAccessor.TryGetUmbracoContext(out umbracoContext);
        this.contentTypeService = contentTypeService;
        this.options = options.Value;
    }

    public LinksObject GetRoot()
    {
        var resp = new LinksObject();
        if (options.ExcludeLinks)
            return resp;

        resp.Links = new Dictionary<string, HalObject>();

        if (options.UmbracoOptions.ExposeMedia)
            resp.Links.Add("media", new HalObject { Name = "Media", Href = "/media" });

        if (options.UmbracoOptions.ExposeMembers)
            resp.Links.Add("members", new HalObject { Name = "Members", Href = "/members" });

        if (options.UmbracoOptions.ExposeMemberGroups)
            resp.Links.Add("memberGroups", new HalObject { Name = "Member Groups", Href = "/memberGroups" });

        if (options.UmbracoOptions.ExposeUsers)
            resp.Links.Add("users", new HalObject { Name = "Users", Href = "/users" });

        if (options.UmbracoOptions.ExposeForms)
            resp.Links.Add("forms", new HalObject { Name = "Forms", Href = "/forms" });

        if (options.UmbracoOptions.ExposeTranslationDictionary)
            resp.Links.Add("dictionary", new HalObject { Name = "Dictionary", Href = "/dictionary" });

        foreach (var contentType in contentTypeService.GetAll())
            resp.Links.Add(contentType.Alias, new HalObject { Name = contentType.Name, Href = contentType.Alias });

        return resp;
    }

    public void Populate(ObjectResponse item)
    {
        if (options.ExcludeLinks)
            return;

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
}