using static Flaeng.Umbraco.ContentAPI.Tests.UmbracoBuilder;

namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class FilterRequestTests : BaseTests
{
    int frontpageId;
    int chessId;
    int checkMateId;

    public FilterRequestTests()
    {
        var builder = UmbracoBuilder.Create();
        builder.AddContentType(
            alias: "contentPage");

        frontpageId = builder.AddContent(
            contentTypeAlias: "contentPage", 
            name: "Frontpage", 
            properties: new List<UmbracoProperty> { new UmbracoProperty("content", null) });
        
        chessId = builder.AddContent(
            contentTypeAlias: "contentPage", 
            name: "chess", 
            parentId: frontpageId, 
            properties: new List<UmbracoProperty> { new UmbracoProperty("content", null) });
        
        checkMateId = builder.AddContent(
            contentTypeAlias: "contentPage", 
            name: "check mate", 
            parentId: frontpageId, 
            properties: new List<UmbracoProperty> { new UmbracoProperty("content", null) });

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

    [Fact]
    public void Can_filter_level_gt()
    {
        QueryString = "$filter=level gt 0";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(2, response.TotalItemCount);
        Assert.True(response.Items.All(x => x.Level == 1));
    }

    [Fact]
    public void Can_filter_level_lt()
    {
        QueryString = "$filter=level lt 1";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.True(response.Items.All(x => x.Level == 0));
    }

    [Fact]
    public void Can_filter_text()
    {
        QueryString = "$filter=name eq Frontpage";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.Equal(frontpageId, response.Items.Single().Id);
    }

    [Fact]
    public void Can_filter_text_with_like()
    {
        QueryString = "$filter=name like frontpage";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.Equal(frontpageId, response.Items.Single().Id);
    }

    [Fact]
    public void Can_filter_text_with_whitespace()
    {
        QueryString = "$filter=name eq check mate";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.Equal(checkMateId, response.Items.Single().Id);
    }

    [Fact]
    public void Can_handle_empty_value()
    {
        QueryString = "$filter=content eq";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.NotEmpty(response.Items);
    }

    [Fact]
    public void Can_have_multiple_filters()
    {
        QueryString = "$filter=level eq 1,name like chess";
        var result = Controller!.Get("contentPage");

        var response = AssertAndGetCollectionResponse(result);

        Assert.Equal(1, response.TotalItemCount);
        Assert.Equal(chessId, response.Items.Single().Id);
    }

}