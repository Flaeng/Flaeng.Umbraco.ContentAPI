namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Initial_collection_response : BaseIntegrationTests
{
    
    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi/employee", initialCollectionResponse["_links"]!["self"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_page_size()
    {
        Assert.Equal(20, initialCollectionResponse.Value<int>("pageSize"));
    }

    [Fact]
    public void Has_page_number()
    {
        Assert.Equal(1, initialCollectionResponse.Value<int>("pageNumber"));
    }

    [Fact]
    public void Has_total_item_count()
    {
        Assert.Equal(95, initialCollectionResponse.Value<int>("totalItemCount"));
    }

    [Fact]
    public void Has_total_page_count()
    {
        Assert.Equal(5, initialCollectionResponse.Value<int>("totalPageCount"));
    }

}