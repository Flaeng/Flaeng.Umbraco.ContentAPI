using System.Text.Json;

using Flaeng.Umbraco.ContentAPI.Converters;
using Flaeng.Umbraco.ContentAPI.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Moq;

using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public abstract class BaseTests
{
    protected ContentApiController? Controller { get; private set; }
    protected IHttpContextAccessor? HttpContextAccessor { get; private set; }

    protected JsonSerializerOptions SerializerOptions => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new ObjectResponseConverter() }
    };

    private QueryCollection Query = new();
    protected string QueryString
    {
        get { return Query.Count == 0 ? String.Empty : String.Join("&", Query.Select(x => $"{x.Key}={x.Value}")); }
        set { Query = new QueryCollection(value.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => new StringValues(x.Skip(1).FirstOrDefault()))); }
    }

    protected void Initialize(IUmbracoApp app)
    {
        var options = new Mock<IOptions<ContentApiOptions>>();
        options.Setup(x => x.Value).Returns(new ContentApiOptions { });

        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary(new Dictionary<string, StringValues> {
            { "content-language", new StringValues("da-dk") }
        }));
        httpRequestMock.Setup(x => x.Query).Returns(() => Query);
        httpRequestMock.Setup(x => x.QueryString).Returns(new QueryString());
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        var outUmbracoContext = app.UmbracoContext;
        umbracoContextAccessorMock
            .Setup(x => x.TryGetUmbracoContext(out outUmbracoContext));

        var filterHelper = new DefaultFilterHandler(httpContextAccessorMock.Object);
        var linkPopulator = new DefaultLinkPopulator(umbracoContextAccessorMock.Object, app.ContentTypeService, options.Object);

        var responseBuilder = new DefaultResponseBuilder(
            httpContextAccessor: httpContextAccessorMock.Object,
            linkPopulator: linkPopulator
        );

        var logger = new Mock<ILogger<ContentApiController>>();

        Controller = new ContentApiController(
            logger: logger.Object,
            umbracoHelper: null,
            umbracoContextAccessor: umbracoContextAccessorMock.Object,
            cache: app.AppCaches,
            options: options.Object,
            responseBuilder: responseBuilder,
            filterHelper: filterHelper,
            linkPopulator: linkPopulator
        );
        var actionContext = new ActionContext(httpContextMock.Object, new RouteData(), new ControllerActionDescriptor());
        Controller.ControllerContext = new(actionContext);
        Controller.ControllerContext.HttpContext = httpContextMock.Object;

        HttpContextAccessor = httpContextAccessorMock.Object;
    }

    protected static ObjectResponse AssertAndGetObjectResponse(ActionResult<object> result)
    {
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        var response = result.Value as ObjectResponse;
        Assert.NotNull(response);
        return response!;
    }

    protected static CollectionResponse AssertAndGetCollectionResponse(ActionResult<object> result)
    {
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        var response = result.Value as CollectionResponse;
        Assert.NotNull(response);
        Assert.NotNull(response!.Items);
        return response!;
    }

}