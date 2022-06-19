namespace Flaeng.Umbraco.ContentAPI.Tests.HalJson;

public class UmbracoMediaRequestTests : BaseTests
{
    public UmbracoMediaRequestTests()
    {
        var builder = UmbracoBuilder.Create();

        Initialize(builder.Build());
    }

    [Fact]
    public void Can_get_media_list()
    {
    }
    
    [Fact]
    public void Can_get_single_media()
    {
    }
    
}