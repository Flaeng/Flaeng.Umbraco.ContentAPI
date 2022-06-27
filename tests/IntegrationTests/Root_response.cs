using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Root_response : BaseIntegrationTests
{
    readonly JToken response;
    public Root_response()
    {
        var result = Controller!.Get(String.Empty);
        var objResult = result.Result as OkObjectResult;
        response = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));
    }

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi", response["_links"]!.Value<string>("self"));
    }

    [Fact]
    public void Has_employee_href()
    {
        Assert.Equal("/api/contentapi/employee", response["_links"]!["employee"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_scrumteam_href()
    {
        Assert.Equal("/api/contentapi/scrumteam", response["_links"]!["scrumteam"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_project_href()
    {
        Assert.Equal("/api/contentapi/project", response["_links"]!["project"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_customer_href()
    {
        Assert.Equal("/api/contentapi/customer", response["_links"]!["customer"]!.Value<string>("href"));
    }

}