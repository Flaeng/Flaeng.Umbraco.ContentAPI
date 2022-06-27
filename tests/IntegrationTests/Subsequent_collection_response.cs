using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Subsequent_collection_response : BaseIntegrationTests
{
    readonly JToken response;
    public Subsequent_collection_response()
    {
        QueryString = "?pageNumber=2";
        var result = Controller!.Get($"employee");
        var objResult = result.Result as OkObjectResult;
        response = JToken.Parse(JsonSerializer.Serialize(objResult?.Value, SerializerOptions));
    }

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi/employee?pageNumber=2", response["_links"]!.Value<string>("self"));
    }

    [Fact]
    public void Has_page_size()
    {
        Assert.Equal(20, response.Value<int>("pageSize"));
    }

    [Fact]
    public void Has_page_number()
    {
        Assert.Equal(2, response.Value<int>("pageNumber"));
    }

    [Fact]
    public void Has_total_item_count()
    {
        Assert.Equal(95, response.Value<int>("totalItemCount"));
    }

    [Fact]
    public void Has_total_page_count()
    {
        Assert.Equal(5, response.Value<int>("totalPageCount"));
    }

}