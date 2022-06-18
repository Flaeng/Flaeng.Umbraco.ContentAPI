namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class TreeObjectRequestTests : BaseTests
{
    int frontpageId;

    public TreeObjectRequestTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage");
        frontpageId = builder.AddContent("contentPage", "Frontpage");
        builder.AddContent("contentPage", "Underside #1", parentId: frontpageId);
        builder.AddContent("contentPage", "Underside #2", parentId: frontpageId);

        builder.AddContentType("settings");
        for (int i = 0; i < 26; i++)
            builder.AddContent("settings", $"Setting #{i}", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void tree_request_returns_children()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response.Items.Count());
    }

    [Fact]
    public void tree_request_returns_total_item_count()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response.TotalItemCount);
    }

    [Fact]
    public void tree_request_returns_page_size()
    {
        QueryString = "$pageSize=1";
        var result = Controller!.Get($"contentPage/{frontpageId}/contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.PageSize);
        Assert.Equal(2, response.TotalPageCount);
    }

    [Fact]
    public void tree_request_returns_page_number()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/settings");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(20, response.Items.Count());
    }

    [Fact]
    public void tree_request_returns_total_item_count_2()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/settings");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(26, response.TotalItemCount);
    }

    [Fact]
    public void tree_request_returns_total_page_count_2()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/settings");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response.TotalPageCount);
    }

    [Fact]
    public void tree_request_returns_object()
    {
        var result = Controller!.Get($"contentPage/{frontpageId}/settings/22");

        var response = AssertAndGetObjectResponse(result);

        Assert.Equal(22, response.Id);
        Assert.Equal("settings", response.ContentType.Alias);
    }

}