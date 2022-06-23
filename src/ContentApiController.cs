using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;

namespace Flaeng.Umbraco.ContentAPI;

[ApiController, Route("api/contentapi")]
public class ContentApiController : UmbracoApiController
{
    protected readonly ILogger<ContentApiController> logger;
    protected readonly UmbracoHelper umbracoHelper;
    protected readonly IUmbracoContext umbracoContext;
    protected readonly AppCaches cache;
    protected readonly IContentApiOptions options;
    protected readonly IResponseBuilder responseBuilder;
    protected readonly IFilterHandler filterHelper;
    protected readonly ILinkPopulator linkPopulator;

    public record BadRequestResponse(string error, string message);
    protected BadRequestResponse NotFoundResponse = new BadRequestResponse("not_found", "Page not found");
    protected string Culture { get => Request.Headers.ContentLanguage.ToString(); }

    public ContentApiController(
            ILogger<ContentApiController> logger,
            UmbracoHelper umbracoHelper,
            IUmbracoContextAccessor umbracoContextAccessor,
            AppCaches cache,
            IOptionsSnapshot<ContentApiOptions> options,
            IResponseBuilder responseBuilder,
            IFilterHandler filterHelper,
            ILinkPopulator linkPopulator
            )
    {
        this.logger = logger;
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
        try
        {
            if (!options.EnableCaching)
                return GetResult(path);

            var cacheKey = $"{Culture}_{path}";
            var result = cache.RuntimeCache.Get(
                    key: cacheKey,
                    factory: () => GetResult(path),
                    timeout: options.CacheTimeout,
                    isSliding: true);

            return result;
        }
        catch (HalException e)
        {
            logger.LogInformation(e, "ContentAPI request returned non-successfull response");
            if (e is ContentTypeNotFoundException || e is ContentNotFoundException)
                return NotFound();

            return BadRequest(new BadRequestResponse(e.SystemMessage, e.Message));
        }
    }

    private object GetResult(string path)
    {
        if (String.IsNullOrWhiteSpace(path))
            return linkPopulator.GetRoot();

        var pathSplit = path.Split('/');
        var rootContentTypeAlias = pathSplit[0];

        if (pathSplit.Length == 1)
        {
            if (tryHandleUmbracoRequest(rootContentTypeAlias, out var result))
            {
                return responseBuilder.Build(rootContentTypeAlias, result);
            }

            var rootContentType = umbracoContext.Content.GetContentType(rootContentTypeAlias);
            if (rootContentType == null)
                throw new ContentTypeNotFoundException(rootContentTypeAlias);

            var contentColl = umbracoContext.Content.GetByContentType(rootContentType);
            contentColl = filterHelper.ApplyFilter(contentColl);
            return responseBuilder.Build(rootContentTypeAlias, contentColl);
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

            return responseBuilder.Build(null, contentColl);
        }
        else
        {
            var contentId = pathSplit[pathSplit.Length - 1];
            var content = GetContentById(contentId, contentType);
            if (content == null)
                throw new ContentNotFoundException(contentType, contentId);

            return responseBuilder.Build(content);
        }
    }

    private bool tryHandleUmbracoRequest(string contentAlias, out IEnumerable<IPublishedContent> result)
    {
        result = null;
        return false;
        // switch (contentAlias.ToLower())
        // {
        //     case "dictionary":
        //         // umbracoHelper.CultureDictionary;
        //         break;

        //     default:
        //         result = null;
        //         return false;
        // }
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