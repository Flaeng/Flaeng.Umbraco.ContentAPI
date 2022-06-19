namespace Flaeng.Umbraco.ContentAPI.Tests;

public class BasicTests
{

    [Fact]
    public void Can_cast_fluent_api_to_options()
    {
        var allow = new ContentApiOptions().AllowCreationOf("test");
        Assert.NotNull((ContentApiOptions)allow);
    }

    [Fact]
    public void Can_hide_links()
    {
        var options = new ContentApiOptions().HideLinks();
        Assert.True(options.HideLinks);
    }

    [Fact]
    public void Can_setup_caching_with_timeout()
    {
        var options = new ContentApiOptions().UseCachingWithTimeout(TimeSpan.FromHours(1));
        Assert.True(options.EnableCaching);
        Assert.Equal(TimeSpan.FromHours(1), options.CacheTimeout);
    }

    [Fact]
    public void Can_setup_smart_caching()
    {
        var options = new ContentApiOptions().UseCachingWithSmartCaching();
        Assert.True(options.EnableCaching);
        Assert.Null(options.CacheTimeout);
        Assert.True(options.SmartCachingIsEnabled);
    }

}