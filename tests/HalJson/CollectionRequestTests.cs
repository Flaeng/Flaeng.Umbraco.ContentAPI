using System.Text.Json;

using Flaeng.Umbraco.ContentAPI.Models;

using Newtonsoft.Json.Linq;

namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class CollectionRequestTests : BaseTests
{
    public CollectionRequestTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage");
        var frontpageId = builder.AddContent("contentPage", "Frontpage");
        for (int i = 0; i < 110; i++)
            builder.AddContent("contentPage", $"Subpage #{i}", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void collection_request_returns_total_item_count()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(111, response!.TotalItemCount);
    }

    [Fact]
    public void collection_request_has_20_items_as_default()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(20, response!.Items.Count());
    }

    [Fact]
    public void collection_request_returns_first_page_as_default()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response!.PageNumber);
    }

    [Fact]
    public void collection_request_can_paginate()
    {
        Path = "/contentPage";
        var result = Controller!.Get("contentPage");
        var resp = result.Value as CollectionResponse;
        var json = JsonSerializer.Serialize(resp, SerializerOptions);
        System.Diagnostics.Debug.WriteLine(json);
        var token = JToken.Parse(json);
        var nextPagePath = token["_links"]!["next"]!["href"]!.ToString();
        var split = nextPagePath.Split('?');

        QueryString = split[1];
        result = Controller!.Get(split[0].TrimStart('/'));
        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response!.PageNumber);
    }

    [Fact]
    public void collection_request_returns_number_of_pages()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(6, response!.TotalPageCount);
    }

    [Fact]
    public void collection_request_handles_nondefault_page_size()
    {
        QueryString = "$pageSize=2";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response!.Items.Count());
        Assert.Equal(2, response!.PageSize);
    }

    [Fact]
    public void collection_response_has_link_to_first_page()
    {
        QueryString = "$pageNumber=5";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);
        var json = JsonSerializer.Serialize(response, SerializerOptions);
        var token = JToken.Parse(json);

        Assert.NotNull(token["_links"]?["first"]?["href"]);
    }

    [Fact]
    public void collection_response_has_link_to_previous_page()
    {
        QueryString = "$pageNumber=5";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);
        var json = JsonSerializer.Serialize(response, SerializerOptions);
        var token = JToken.Parse(json);

        Assert.NotNull(token["_links"]?["previous"]?["href"]);
    }

    [Fact]
    public void collection_response_has_link_to_next_page()
    {
        QueryString = "$pageNumber=5";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);
        var json = JsonSerializer.Serialize(response, SerializerOptions);
        var token = JToken.Parse(json);

        Assert.NotNull(token["_links"]?["next"]?["href"]);
    }

    [Fact]
    public void collection_response_has_link_to_last_page()
    {
        QueryString = "$pageNumber=5";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);
        var json = JsonSerializer.Serialize(response, SerializerOptions);
        var token = JToken.Parse(json);

        Assert.NotNull(token["_links"]?["last"]?["href"]);
    }

}