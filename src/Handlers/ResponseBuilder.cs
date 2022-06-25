using System;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.Extensions;

using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Handlers;
public interface IResponseBuilder
{
    ObjectResponse Build(ObjectInterpreterResult request);
    CollectionResponse Build(CollectionInterpreterResult request);
    LinksObject BuildRoot(RootInterpreterResult request);
}
public class DefaultResponseBuilder : IResponseBuilder
{
    protected readonly IUmbracoContext umbracoContext;
    protected readonly HttpContext httpContext;
    protected HttpRequest Request => httpContext.Request;
    protected string Culture => Request.Headers.ContentLanguage;
    protected readonly ILinkPopulator linkPopulator;
    protected readonly IExpander expander;

    public DefaultResponseBuilder(
        IUmbracoContextAccessor umbracoContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ILinkPopulator linkPopulator,
        IExpander expander
        )
    {
        this.umbracoContext = umbracoContextAccessor.GetUmbracoContextOrThrow();
        this.httpContext = httpContextAccessor.HttpContext;
        this.linkPopulator = linkPopulator;
        this.expander = expander;
    }

    public virtual ObjectResponse Build(ObjectInterpreterResult request)
    {
        var result = new ObjectResponse(request.Value, Culture);
        expander.Expand(result);
        linkPopulator.Populate(result);
        return result;
    }

    public virtual CollectionResponse Build(CollectionInterpreterResult request)
    {
        var contentColl = request.Value;

        int pageNumber, pageSize;
        
        if (int.TryParse(Request.Query.GetValue("pageNumber", "1"), out pageNumber) == false)
            throw new InvalidQueryParameterException("pageNumber");

        if (int.TryParse(Request.Query.GetValue("pageSize", "20"), out pageSize) == false)
            throw new InvalidQueryParameterException("pageSize");

        string orderBy = Request.Query.GetValue("orderBy");
        if (String.IsNullOrWhiteSpace(orderBy) == false)
        {
            var contentType = umbracoContext.Content.GetContentType(request.ContentTypeAlias);
            if (contentType.PropertyTypes.Any(x => x.Alias == orderBy) == false)
                throw new InvalidQueryParameterException("orderBy");

            contentColl = contentColl.OrderBy(x => x.GetProperty(orderBy).GetValue(Culture));
        }

        var result = new CollectionResponse(request.ContentTypeAlias, Culture, contentColl, pageNumber, pageSize);
        expander.Expand(result);
        linkPopulator.Populate(result);
        return result;
    }

    public virtual LinksObject BuildRoot(RootInterpreterResult request)
    {
        var result = new LinksObject();
        linkPopulator.PopulateRoot(result);
        return result;
    }
}