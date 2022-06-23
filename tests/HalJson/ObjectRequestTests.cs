namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class ObjectRequestTests : BaseTests
{
    readonly int subpage2Id;

    public ObjectRequestTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage", "Content Page");
        var frontpageId = builder.AddContent("contentPage", "Frontpage");
        builder.AddContent("contentPage", "Underside #1", parentId: frontpageId);
        subpage2Id = builder.AddContent("contentPage", "Underside #2", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void object_request_returns_content_id()
    {
        var result = Controller!.Get($"contentPage/{subpage2Id}");

        var response = AssertAndGetObjectResponse(result);

        Assert.Equal(subpage2Id, response.Id);
    }

    [Fact]
    public void object_request_returns_content_name()
    {
        var result = Controller!.Get($"contentPage/{subpage2Id}");

        var response = AssertAndGetObjectResponse(result);

        Assert.Equal("Underside #2", response.Name);
    }

    [Fact]
    public void object_request_returns_content_level()
    {
        var result = Controller!.Get($"contentPage/{subpage2Id}");

        var response = AssertAndGetObjectResponse(result);

        Assert.Equal(1, response.Level);
    }

}