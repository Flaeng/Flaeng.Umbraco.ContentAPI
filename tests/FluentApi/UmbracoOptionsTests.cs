namespace Flaeng.Umbraco.ContentAPI.Tests;

public class UmbracoOptionsTests
{
    [Fact]
    public void Can_set_expose_media()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeMedia);
        options.ExposeMedia();
        Assert.True(options.UmbracoOptions.ExposeMedia);
    }
    
    [Fact]
    public void Can_set_expose_members()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeMembers);
        options.ExposeMembers();
        Assert.True(options.UmbracoOptions.ExposeMembers);
    }
    
    [Fact]
    public void Can_set_expose_member_groups()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeMemberGroups);
        options.ExposeMemberGroups();
        Assert.True(options.UmbracoOptions.ExposeMemberGroups);
    }
    
    [Fact]
    public void Can_set_expose_users()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeUsers);
        options.ExposeUsers();
        Assert.True(options.UmbracoOptions.ExposeUsers);
    }
    
    [Fact]
    public void Can_set_expose_forms()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeForms);
        options.ExposeForms();
        Assert.True(options.UmbracoOptions.ExposeForms);
    }
    
    [Fact]
    public void Can_set_expose_translation_dictionary()
    {
        var options = new ContentApiOptions();
        Assert.False(options.UmbracoOptions.ExposeTranslationDictionary);
        options.ExposeTranslationDictionary();
        Assert.True(options.UmbracoOptions.ExposeTranslationDictionary);
    }
    
}