using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Flaeng.Umbraco.ContentAPI.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;

namespace Flaeng.Umbraco.ContentAPI;

[ApiController, Route("api/contentapi")]
public class ContentApiController : UmbracoApiController
{
    protected readonly UmbracoHelper umbracoHelper;
    protected readonly IUmbracoContext umbracoContext;
    protected readonly AppCaches cache;
    protected readonly ContentApiOptions options;
    protected readonly IResponseBuilder responseBuilder;
    protected readonly IFilterHelper filterHelper;
    protected readonly ILinkPopulator linkPopulator;

    protected object NotFoundResponse = new
    {
        errorCode = 404,
        error = "not_found",
        message = "Page not found"
    };
    protected string Culture { get => Request.Headers.ContentLanguage.ToString(); }

    public ContentApiController(
            UmbracoHelper umbracoHelper,
            IUmbracoContextAccessor umbracoContextAccessor,
            AppCaches cache,
            IOptions<ContentApiOptions> options,
            IResponseBuilder responseBuilder,
            IFilterHelper filterHelper,
            ILinkPopulator linkPopulator
            )
    {
        this.umbracoHelper = umbracoHelper;
        umbracoContextAccessor.TryGetUmbracoContext(out this.umbracoContext);
        this.cache = cache;
        this.options = options.Value;
        this.responseBuilder = responseBuilder;
        this.filterHelper = filterHelper;
        this.linkPopulator = linkPopulator;
    }

    [HttpGet, Route("{*path}")]
    public ActionResult<object> Get(string path)
    {
        if (options.DisableCaching)
            return GetResult(path);

        var cacheKey = $"{Culture}_{path}";
        var result = cache.RuntimeCache.Get(
                key: cacheKey,
                factory: () => GetResult(path),
                timeout: options.CacheTimeout,
                isSliding: true);

        return result;
    }

    private object GetResult(string path)
    {
        if (String.IsNullOrWhiteSpace(path))
            return linkPopulator.GetRoot();

        var pathSplit = path.Split('/');
        var rootContentTypeAlias = pathSplit[0];

        if (pathSplit.Length == 1)
        {
            var rootContentType = umbracoContext.Content.GetContentType(rootContentTypeAlias);
            if (rootContentType == null)
                return NotFound();

            var contentColl = umbracoContext.Content.GetByContentType(rootContentType);
            contentColl = filterHelper.ApplyFilter(contentColl);
            return responseBuilder.Build(contentColl);
        }

        var isCollectionResponse = pathSplit.Length % 2 == 1;
        var contentType = pathSplit[pathSplit.Length - (pathSplit.Length % 2 == 1 ? 1 : 2)];

        if (isCollectionResponse)
        {
            var parentContentType = pathSplit[pathSplit.Length - (pathSplit.Length % 2) - 2];
            var parentContentId = pathSplit[pathSplit.Length - (pathSplit.Length % 2) - 1];

            var content = GetContentById(parentContentId, parentContentType);

            IEnumerable<IPublishedContent> contentColl;
            var property = content.GetProperty(contentType);
            if (property != null)
                contentColl = property.GetValue() as IEnumerable<IPublishedContent>;
            else 
                contentColl = content.Children.Where(x => x.ContentType.Alias == contentType);

            contentColl = filterHelper.ApplyFilter(contentColl);

            return responseBuilder.Build(contentColl);
        }
        else
        {
            var contentId = pathSplit[pathSplit.Length - 1];
            var content = GetContentById(contentId, contentType);
            if (content == null)
                return NotFound();

            return responseBuilder.Build(content);
        }
    }

    private IPublishedContent GetContentById(string contentId, string contentTypeAlias)
    {
        var content = 
            int.TryParse(contentId, out int contentIntId)
            ? umbracoContext.Content.GetById(contentIntId)
            
            : Guid.TryParse(contentId, out Guid contentGuidId)
            ? umbracoContext.Content.GetById(contentGuidId)
            
            : umbracoContext.Content.GetById(Udi.Create(contentTypeAlias, contentId));

        return content == null || content.ContentType.Alias != contentTypeAlias
            ? null
            : content;
    }

}