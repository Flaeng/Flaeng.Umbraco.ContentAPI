namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Subsequent_collection_response : BaseIntegrationTests
{

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi/employee?pageNumber=2", subsequentCollectionResponse["_links"]!["self"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_page_size()
    {
        Assert.Equal(20, subsequentCollectionResponse.Value<int>("pageSize"));
    }

    [Fact]
    public void Has_page_number()
    {
        Assert.Equal(2, subsequentCollectionResponse.Value<int>("pageNumber"));
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