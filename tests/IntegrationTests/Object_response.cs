using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Object_response : BaseIntegrationTests
{
    readonly JToken response;
    public Object_response()
    {
        var result = Controller!.Get($"scrumteam/8");
        var objResult = result.Result as OkObjectResult;
        response = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));
    }

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi/scrumteam/8", response["_links"]!.Value<string>("self"));
    }

    [Fact]
    public void Has_id()
    {
        Assert.Equal(8, response.Value<int>("id"));
    }

    [Fact]
    public void Has_name()
    {
        Assert.Equal("Team Delta", response.Value<string>("name"));
    }

    [Fact]
    public void Has_level()
    {
        Assert.Equal(2, response.Value<int>("level"));
    }

    [Fact]
    public void Has_custom_property()
    {
        Assert.Equal("01/13/2020 00:00:00", response.Value<string>("startedAt"));
    }

}