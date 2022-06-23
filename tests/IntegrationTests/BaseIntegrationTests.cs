using System.Text.Json;

using Moq;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Models.PublishedContent;

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
        rootRequestResponse = JToken.Parse(JsonSerializer.Serialize(result.Value, SerializerOptions));

        result = Controller!.Get($"employee");
        initialCollectionResponse = JToken.Parse(JsonSerializer.Serialize(result.Value, SerializerOptions));

        QueryString = "?pageNumber=2";
        result = Controller!.Get($"employee");
        subsequentCollectionResponse = JToken.Parse(JsonSerializer.Serialize(result.Value, SerializerOptions));

        QueryString = String.Empty;
        result = Controller!.Get($"scrumteam/8");
        objectResponse = JToken.Parse(JsonSerializer.Serialize(result.Value, SerializerOptions));
    }

}