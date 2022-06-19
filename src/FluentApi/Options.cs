
using System;

namespace Flaeng.Umbraco.ContentAPI;

public interface IAllowCrudOperation
{
    ContentApiOptions If(Func<CrudRequest, bool> condition);
}
public class AllowCrudOf : IAllowCrudOperation
{
    public ContentApiOptions Options { get; }
    public ContentTypeCrudOption CrudOption { get; }

    public AllowCrudOf(ContentApiOptions Options, ContentTypeCrudOption CrudOption)
    {
        this.Options = Options;
        this.CrudOption = CrudOption;
    }

    public ContentApiOptions If(Func<CrudRequest, bool> condition)
    {
        this.CrudOption.Authorize = condition;
        return this.Options;
    }
    
    public static implicit operator ContentApiOptions(AllowCrudOf aco) => aco.Options;
}
public static class ContentApiOptionsExtensions
{
    public static AllowCrudOf AllowCreationOf(
        this ContentApiOptions options,
        string contentTypeAlias
        )
    {
        var crudOption = new ContentTypeCrudOption
        {
            ContentTypeAlias = contentTypeAlias
        };
        options.CreationOptions.Add(crudOption);
        return new AllowCrudOf(options, crudOption);
    }

    public static ContentApiOptions ExposeMedia(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeMedia = true;
        return options;
    }

    public static ContentApiOptions ExposeMembers(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeMembers = true;
        return options;
    }

    public static ContentApiOptions ExposeMemberGroups(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeMemberGroups = true;
        return options;
    }

    public static ContentApiOptions ExposeUsers(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeUsers = true;
        return options;
    }

    public static ContentApiOptions ExposeForms(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeForms = true;
        return options;
    }

    public static ContentApiOptions ExposeTranslationDictionary(this ContentApiOptions options)
    {
        options.UmbracoOptions.ExposeTranslationDictionary = true;
        return options;
    }

    public static ContentApiOptions HideLinks(this ContentApiOptions options)
    {
        options.HideLinks = true;
        return options;
    }

    public static ContentApiOptions UseCachingWithTimeout(
        this ContentApiOptions options,
        TimeSpan timeout
        )
    {
        options.EnableCaching = true;
        options.CacheTimeout = timeout;
        return options;
    }

    public static ContentApiOptions UseCachingWithSmartCaching(
        this ContentApiOptions options
        )
    {
        options.EnableCaching = true;
        options.CacheTimeout = null;
        return options;
    }
}

