namespace Flaeng.Umbraco.ContentAPI.Tests.IntegrationTests;

public class Object_response : BaseIntegrationTests
{

    [Fact]
    public void Has_self_link()
    {
        Assert.Equal("/api/contentapi/scrumteam/8", objectResponse["_links"]!["self"]!.Value<string>("href"));
    }

    [Fact]
    public void Has_id()
    {
        Assert.Equal(8, objectResponse.Value<int>("id"));
    }

    [Fact]
    public void Has_name()
    {
        Assert.Equal("Team Delta", objectResponse.Value<string>("name"));
    }

    [Fact]
    public void Has_level()
    {
        Assert.Equal(2, objectResponse.Value<int>("level"));
    }

    [Fact]
    public void Has_custom_property()
    {
        Assert.Equal("01/13/2020 00:00:00", objectResponse.Value<string>("startedAt"));
    }

}