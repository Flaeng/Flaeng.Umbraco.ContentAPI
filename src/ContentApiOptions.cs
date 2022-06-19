using System;
using System.Collections.Generic;

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;

public class ContentApiOptions
{
    public bool EnableCaching { get; set; }
    public TimeSpan? CacheTimeout { get; set; } = TimeSpan.FromHours(1);

    public bool ExcludeLinks { get; set; }

    public UmbracoOptions UmbracoOptions { get; set; } = new UmbracoOptions();

    public CrudOptions CreationOptions { get; set; }
    public CrudOptions EditingOptions { get; set; }
    public CrudOptions DeletionOptions { get; set; }
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
public class CrudOptions
{
    public List<string> ContentTypes { get; set; }
    public Func<CrudRequest, bool> Authorize { get; set; }
}
public class CrudRequest
{
    public IPublishedContent Content { get; set; }
}