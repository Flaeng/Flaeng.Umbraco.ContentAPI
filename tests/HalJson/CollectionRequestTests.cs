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
        QueryString = "$pageNumber=3";
        var result = Controller!.Get("contentPage");
        
        var response = AssertAndGetCollectionResponse(result);
        
        Assert.Equal(3, response!.PageNumber);
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

    // [Fact]
    // public void collection_response_has_link_to_first_page()
    // {
    // }

    // [Fact]
    // public void collection_response_has_link_to_previous_page()
    // {
    // }

    // [Fact]
    // public void collection_response_has_link_to_next_page()
    // {
    // }

    // [Fact]
    // public void collection_response_has_link_to_last_page()
    // {
    // }

}