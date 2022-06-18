namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class FilterRequestTests : BaseTests
{
    int frontpageId;

    public FilterRequestTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType("contentPage");
        frontpageId = builder.AddContent("contentPage", "Frontpage");
        builder.AddContent("contentPage", "Underside #1", parentId: frontpageId);
        builder.AddContent("contentPage", "Underside #2", parentId: frontpageId);

        Initialize(builder.Build());
    }

    [Fact]
    public void Can_filter_id_eq()
    {
        QueryString = "$filter=id eq 1";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.Equal(1, response.Items.Single().Id);
    }
    
    [Fact]
    public void Can_filter_level_eq()
    {
        QueryString = "$filter=level eq 1";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response.TotalItemCount);
        Assert.True(response.Items.All(x => x.Level == 1));
    }

}