using Flaeng.Umbraco.ContentAPI.Options;

namespace Flaeng.Umbraco.ContentAPI.Tests;

public class CrudOptionsTests
{
    [Fact]
    public void Can_allow_creation()
    {
        var options = new ContentApiOptions();
        var options2 = options.AllowCreationOf("blogPostComment");
        var options3 = options2.If(x => true);
        Assert.Equal(options, options2);
        Assert.Equal(options, options3);
        Assert.Contains(options.CreationOptions, x => x.ContentTypeAlias == "blogPostComment" && x.Authorize != null);
    }

    [Fact]
    public void Can_allow_creation_with_authorization()
    {
        var options = new ContentApiOptions();
        var options2 = options.AllowCreationOf("blogPostComment");
        Assert.Equal(options, options2);
        Assert.Contains(options.CreationOptions, x => x.ContentTypeAlias == "blogPostComment" && x.Authorize == null);
    }
}