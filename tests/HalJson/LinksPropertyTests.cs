using System.Text.RegularExpressions;

using Flaeng.Umbraco.ContentAPI.Models;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public class LinksPropertyTests : BaseTests
{
    int frontpageId;

    public LinksPropertyTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage", "Content Page");
        frontpageId = builder.AddContent("contentPage", "Frontpage");
        for (int i = 0; i < 110; i++)
            builder.AddContent("contentPage", $"Subpage #{i}", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void RootRequest_has_links_self()
    {
        var result = Controller!.Get(String.Empty);

        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        var response = result.Value as LinksObject;
        Assert.NotNull(response);

        Assert.Equal("/api/contentapi", response!.Links["self"].Href);
    }

    [Fact]
    public void Collection_responses_has_links_self()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal("/api/contentapi/contentPage", response.Links["self"].Href);
    }

    [Fact]
    public void Collection_response_items_has_links_self()
    {
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.All(response.Items, x => Assert.Equal($"/api/contentapi/contentPage/{x.Id}", x.Links["self"].Href));
    }

    [Fact]
    public void Object_responses_has_links_self()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}");

        var response = AssertAndGetObjectResponse(result);

        Assert.Equal($"/api/contentapi/contentPage/{frontpageId}", response.Links["self"].Href);
    }

}