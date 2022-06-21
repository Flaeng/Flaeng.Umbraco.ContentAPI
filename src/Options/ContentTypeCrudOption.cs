using System;

namespace Flaeng.Umbraco.ContentAPI.Options;

public class ContentTypeCrudOption
{
    public string ContentTypeAlias { get; init; }
    public Func<CrudRequest, bool> Authorize { get; set; }
}
