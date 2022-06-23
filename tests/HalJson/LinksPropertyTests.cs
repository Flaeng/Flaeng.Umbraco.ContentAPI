namespace Flaeng.Umbraco.ContentAPI.Tests;

public class LinksPropertyTests : BaseTests
{
    public LinksPropertyTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage");
        var frontpageId = builder.AddContent("contentPage", "Frontpage");
        for (int i = 0; i < 110; i++)
            builder.AddContent("contentPage", $"Subpage #{i}", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void Responses_has_links_self()
    {
        var result = Controller!.Get(String.Empty);

        var response = AssertAndGetCollectionResponse(result);

        Assert.NotNull(response.Links);
        Assert.NotNull(response.Links["self"]);
        Assert.NotNull(response.Links["self"].Href);
    }

}