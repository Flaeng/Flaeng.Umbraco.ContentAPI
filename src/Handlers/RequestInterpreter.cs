using System;
using System.Collections.Generic;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.ContentAPI.Options;
using Flaeng.Umbraco.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface IInterpreterResult { }
public interface IInterpreterResult<T> : IInterpreterResult { T Value { get; } }
public record RootInterpreterResult() : IInterpreterResult;
public record UmbracoInterpreterResult(IDictionary<string, object> Value)
    : IInterpreterResult<IDictionary<string, object>>;

public record ObjectInterpreterResult(string ContentTypeAlias, IPublishedContent Value)
    : IInterpreterResult<IPublishedContent>;

public record CollectionInterpreterResult(string ContentTypeAlias, IEnumerable<IPublishedElement> Value)
    : IInterpreterResult<IEnumerable<IPublishedElement>>;
public interface IRequestInterpreter
{
    IInterpreterResult Interprete(string localPath);
}
public class DefaultRequestInterpreter : IRequestInterpreter
{
    protected readonly ILogger<DefaultRequestInterpreter> logger;
    protected readonly ContentApiOptions options;
    protected readonly IUmbracoContext umbracoContext;
    protected readonly UmbracoHelper umbracoHelper;
    protected readonly IFilterInterpreter filterInterpreter;

    public DefaultRequestInterpreter(
        ILogger<DefaultRequestInterpreter> logger,
        IOptions<ContentApiOptions> options,
        IUmbracoContextAccessor umbracoContextAccessor,
        UmbracoHelper umbracoHelper,
        IFilterInterpreter filterInterpreter
        )
    {
        this.logger = logger;
        this.options = options.Value;
        this.umbracoContext = umbracoContextAccessor.GetUmbracoContextOrThrow();
        this.umbracoHelper = umbracoHelper;
        this.filterInterpreter = filterInterpreter;
    }

    public virtual IInterpreterResult Interprete(string localPath)
    {
        if (String.IsNullOrWhiteSpace(localPath))
            return new RootInterpreterResult();

        var pathSplit = localPath.Split('/');
        IInterpreterResult result;

        if (tryInterpreteUmbracoRequest(pathSplit, out result))
            return result;

        if (TryGetUmbracoItem(umbracoContext.Content, pathSplit, out result))
            return result;

        return null;
    }

    public virtual bool TryGetUmbracoItem(IPublishedCache cache, string[] pathSplit, out IInterpreterResult result)
    {
        result = null;
        var rootContentTypeAlias = pathSplit[0];

        if (pathSplit.Length == 1)
        {
            var rootContentType = umbracoContext.Content.GetContentType(rootContentTypeAlias);
            if (rootContentType == null)
            {
                logger.LogWarning($"Failed to find contenttype '{rootContentTypeAlias}'");
                return false;
            }

            var contentColl = umbracoContext.Content.GetByContentType(rootContentType);

            contentColl = filterInterpreter.ApplyFilter(contentColl);

            result = new CollectionInterpreterResult(rootContentTypeAlias, contentColl);
            return true;
        }

        var isCollectionResponse = pathSplit.Length % 2 == 1;
        var contentType = pathSplit[pathSplit.Length - (pathSplit.Length % 2 == 1 ? 1 : 2)];

        if (isCollectionResponse)
        {
            var parentContentType = pathSplit[pathSplit.Length - (pathSplit.Length % 2) - 2];
            var parentContentId = pathSplit[pathSplit.Length - (pathSplit.Length % 2) - 1];

            var content = GetContentById(parentContentId, parentContentType);
            if (content == null)
            {
                logger.LogWarning($"Failed to find content of type '{contentType}' with id '{parentContentId}'");
                return false;
            }

            IEnumerable<IPublishedElement> contentColl;
            var property = content.GetProperty(contentType);
            if (property != null)
                contentColl = property.GetValue() as IEnumerable<IPublishedElement>;
            else
                contentColl = content.Children.Where(x => x.ContentType.Alias == contentType);

            contentColl = filterInterpreter.ApplyFilter(contentColl);

            result = new CollectionInterpreterResult(contentType, contentColl ?? new List<IPublishedElement>());
            return true;
        }
        else
        {
            var contentId = pathSplit[pathSplit.Length - 1];
            var content = GetContentById(contentId, contentType);
            if (content == null)
            {
                logger.LogWarning($"Failed to find content of type '{contentType}' with id '{contentId}'");
                return false;
            }

            result = new ObjectInterpreterResult(contentType, content);
            return true;
        }
    }

    public virtual IPublishedContent GetContentById(string contentId, string contentTypeAlias)
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

    public virtual bool tryInterpreteUmbracoRequest(string[] request, out IInterpreterResult result)
    {
        // var opts = options.UmbracoOptions;
        result = null;
        switch (request[0].ToLower())
        {
            // case "media":
            //     return opts.ExposeMedia == false
            //         ? false
            //         : TryGetUmbracoItem(umbracoContext.Media, request, out result);

            // case "members":
            //     if (opts.ExposeMembers == false)
            //         return false;
            //     throw new NotImplementedException();

            // case "membergroups":
            //     if (opts.ExposeMemberGroups == false)
            //         return false;
            //     throw new NotImplementedException();

            // case "users":
            //     if (opts.ExposeUsers == false)
            //         return false;
            //     throw new NotImplementedException();

            // case "forms":
            //     if (opts.ExposeForms == false)
            //         return false;
            //     throw new NotImplementedException();

            // case "dictionary":
            //     if (opts.ExposeTranslationDictionary == false)
            //         return false;
            //     throw new NotImplementedException();

            default:
                return false;
        }
    }

}