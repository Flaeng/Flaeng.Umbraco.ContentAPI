using System.Text.Json;

using Flaeng.Umbraco.ContentAPI.Handlers;
using Flaeng.Umbraco.ContentAPI.Models;
using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Moq;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public abstract class BaseIntegrationTests : BaseTests
{
    protected JToken rootRequestResponse;
    protected JToken objectResponse;
    protected JToken initialCollectionResponse;
    protected JToken subsequentCollectionResponse;

    public BaseIntegrationTests()
    {
        var builder = UmbracoBuilder.Create();

        int ct_employeeId = builder.AddContentType("employee", "Employee");
        int ct_teamId = builder.AddContentType("scrumteam", "Scrum Team", childrenContentTypeId: new[] { ct_employeeId });
        int ct_projectId = builder.AddContentType("project", "Project", childrenContentTypeId: new[] { ct_teamId });
        int ct_customerId = builder.AddContentType("customer", "Customer", childrenContentTypeId: new[] { ct_projectId });
        int ct_customerListId = builder.AddContentType("customerList", "Customer List", childrenContentTypeId: new[] { ct_customerId });

        int customerListId = builder.AddContent("customerList", "Customers");

        int firstCustomerId = builder.AddContent("customer", "Our Very First Customer", customerListId);
        int secondCustomerId = builder.AddContent("customer", "Our Second Customer", customerListId);
        int thirdCustomerId = builder.AddContent("customer", "Our Third Customer", customerListId);

        builder.AddContent("scrumteam", "Team Alpha", firstCustomerId);
        builder.AddContent("scrumteam", "Team Beta", secondCustomerId);
        builder.AddContent("scrumteam", "Team Charlie", thirdCustomerId);
        int deltaId = builder.AddContent("scrumteam", "Team Delta", firstCustomerId, new List<UmbracoBuilder.UmbracoProperty>
        {
            new UmbracoBuilder.UmbracoProperty(new Mock<IPublishedPropertyType>().Object, "startedAt", new DateTime(2020, 1, 13))
        });
        builder.AddContent("scrumteam", "Team Echo", thirdCustomerId);

        for (int i = 0; i < 95; i++)
            builder.AddContent("employee", $"Employee #{i}", deltaId);

        Initialize(builder.Build());

        var result = Controller!.Get(String.Empty);
        var objResult = result.Result as OkObjectResult;
        rootRequestResponse = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));

        result = Controller!.Get($"employee");
        objResult = result.Result as OkObjectResult;
        initialCollectionResponse = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));

        QueryString = "?pageNumber=2";
        result = Controller!.Get($"employee");
        objResult = result.Result as OkObjectResult;
        subsequentCollectionResponse = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));

        QueryString = String.Empty;
        result = Controller!.Get($"scrumteam/8");
        objResult = result.Result as OkObjectResult;
        objectResponse = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));
    }

    protected ContentApiController? Controller { get; private set; }
    protected IHttpContextAccessor? HttpContextAccessor { get; private set; }

    protected JsonSerializerOptions SerializerOptions => new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        // Converters = { new ObjectResponseConverter(), new CollectionResponseConverter() }
    };

    private QueryCollection Query = new();
    protected string QueryString
    {
        get { return Query.Count == 0 ? String.Empty : "?" + String.Join("&", Query.Select(x => $"{x.Key}={x.Value}")); }
        set { Query = value.TrimStart('?').Length == 0 ? new QueryCollection() : new QueryCollection(value.TrimStart('?').Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => new StringValues(x.Skip(1).FirstOrDefault()))); }
    }

    protected string Path { get; set; } = "";

    protected void Initialize(IUmbracoApp app, Action<ContentApiOptions>? configureOptions = null)
    {
        var optionsMock = new Mock<IOptions<ContentApiOptions>>();
        var options = new ContentApiOptions();
        configureOptions?.Invoke(options);
        optionsMock.Setup(x => x.Value).Returns(options);

        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
        httpRequestMock.Setup(x => x.Headers).Returns(new HeaderDictionary(new Dictionary<string, StringValues> {
            { "content-language", new StringValues("da-dk") }
        }));
        httpRequestMock.Setup(x => x.Path).Returns(() => $"/" + Path.TrimStart('/'));
        httpRequestMock.Setup(x => x.Query).Returns(() => Query);
        httpRequestMock.Setup(x => x.QueryString).Returns(() => new QueryString(QueryString));
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        var outUmbracoContext = app.UmbracoContext;
        umbracoContextAccessorMock
            .Setup(x => x.TryGetUmbracoContext(out outUmbracoContext))
            .Returns(() => true);

        var linkFormatter = new DefaultLinkFormatter(
            optionsMock.Object,
            httpContextAccessorMock.Object);

        var filterInterpreter = new DefaultFilterInterpreter(httpContextAccessorMock.Object);

        var linkPopulator = new DefaultLinkPopulator(
            httpContextAccessorMock.Object,
            umbracoContextAccessorMock.Object,
            app.ContentTypeService,
            optionsMock.Object,
            linkFormatter);

        var responseBuilder = new DefaultResponseBuilder(
            umbracoContextAccessorMock.Object,
            httpContextAccessorMock.Object,
            linkPopulator
        );

        var requestInterpreter = new DefaultRequestInterpreter(
            new Mock<ILogger<DefaultRequestInterpreter>>().Object,
            optionsMock.Object,
            umbracoContextAccessorMock.Object,
            app.UmbracoHelper,
            filterInterpreter
        );

        var logger = new Mock<ILogger<ContentApiController>>();

        Controller = new ContentApiController(
            logger.Object,
            app.AppCaches,
            optionsMock.Object,
            responseBuilder,
            requestInterpreter
        );

        var actionContext = new ActionContext(
            httpContextMock.Object,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new ControllerActionDescriptor());

        Controller.ControllerContext = new ControllerContext(actionContext);
        Controller.ControllerContext.HttpContext = httpContextMock.Object;

        HttpContextAccessor = httpContextAccessorMock.Object;
    }

    protected static HalObject AssertAndGetObjectResponse(ActionResult<object> result)
    {
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        var response = result.Value as HalObject;
        Assert.NotNull(response);
        return response!;
    }

    protected static HalCollection AssertAndGetCollectionResponse(ActionResult<object> result)
    {
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        var response = result.Value as HalCollection;
        Assert.NotNull(response);
        Assert.NotNull(response!.Items);
        return response!;
    }

}