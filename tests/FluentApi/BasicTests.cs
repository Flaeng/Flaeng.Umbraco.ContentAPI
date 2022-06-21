using Flaeng.Umbraco.ContentAPI.Options;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public class BasicTests
{

    [Fact]
    public void Can_add_creation_option()
    {
        IContentApiOptions options = new ContentApiOptions();
        Assert.Empty(options.CreationOptions);

        IAllowCrudOperation opr;
        options = opr = options.AllowCreationOf("test");
        Assert.Contains(options.CreationOptions,
            x => x.ContentTypeAlias == "test" && x.Authorize == null);

        opr.If(x => true);
        Assert.Contains(options.CreationOptions,
            x => x.ContentTypeAlias == "test" && x.Authorize != null);
    }

    [Fact]
    public void Can_add_editing_option()
    {
        IContentApiOptions options = new ContentApiOptions();
        Assert.Empty(options.EditingOptions);

        IAllowCrudOperation opr;
        options = opr = options.AllowEditingOf("test");
        Assert.Contains(options.EditingOptions,
            x => x.ContentTypeAlias == "test" && x.Authorize == null);

        opr.If(x => true);
        Assert.Contains(options.EditingOptions,
            x => x.ContentTypeAlias == "test" && x.Authorize != null);
    }

    [Fact]
    public void Can_hide_links()
    {
        IContentApiOptions options = new ContentApiOptions();
        Assert.Equal(default, options.HideLinks);

        options = options.IgnoreLinks();
        Assert.True(options.HideLinks);
    }

    [Fact]
    public void Can_setup_caching_with_timeout()
    {
        IContentApiOptions options = new ContentApiOptions();
        Assert.Equal(default, options.EnableCaching);
        Assert.Equal(default, options.CacheTimeout);

        options = options.UseCachingWithTimeout(TimeSpan.FromHours(1));
        Assert.True(options.EnableCaching);
        Assert.Equal(TimeSpan.FromHours(1), options.CacheTimeout);
    }

    [Fact]
    public void Can_setup_smart_caching()
    {
        IContentApiOptions options = new ContentApiOptions();
        Assert.Equal(default, options.EnableSmartCaching);

        options = options.UseCachingWithSmartCaching();
        Assert.True(options.EnableSmartCaching);
    }

}