using System;
using System.Collections.Generic;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;

public class ContentApiOptions
{
    public bool EnableCaching { get; set; }
    public TimeSpan? CacheTimeout { get; set; }

    public bool SmartCachingIsEnabled 
    { 
        get => EnableCaching == true && CacheTimeout == null; 
        set 
        {
            if (value)
            {
                EnableCaching = true;
                CacheTimeout = null;
            }
            else EnableCaching = false;
        }
    }

    public bool HideLinks { get; set; }

    public UmbracoOptions UmbracoOptions { get; } = new UmbracoOptions();

    public CrudOptions CreationOptions { get; } = new CrudOptions();
    public CrudOptions EditingOptions { get; } = new CrudOptions();
    public CrudOptions DeletionOptions { get; } = new CrudOptions();
}
public class UmbracoOptions
{
    public bool ExposeMedia { get; set; }
    public bool ExposeMembers { get; set; }
    public bool ExposeMemberGroups { get; set; }
    public bool ExposeUsers { get; set; }
    public bool ExposeForms { get; set; }
    public bool ExposeTranslationDictionary { get; set; }
}
public class CrudOptions : List<ContentTypeCrudOption>
{
}
public class ContentTypeCrudOption
{
    public string ContentTypeAlias { get; init; }
    public Func<CrudRequest, bool> Authorize { get; set; }
}
public class CrudRequest
{
    public IPublishedContent Content { get; init; }
}