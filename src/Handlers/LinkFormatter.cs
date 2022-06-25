using System;

using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface ILinkFormatter
{
    string FormatHref(string localPath);
}
public class DefaultLinkFormatter : ILinkFormatter
{
    protected readonly ContentApiOptions options;
    protected readonly HttpContext httpContext;

    public DefaultLinkFormatter(
        IOptions<ContentApiOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        this.options = options.Value;
        this.httpContext = httpContextAccessor.HttpContext;
    }

    public virtual string FormatHref(string localPath)
    {
        var head = options.PrependAbsolutePathToLinks ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" : String.Empty;
        var tail = String.IsNullOrWhiteSpace(localPath) ? "/api/contentapi" : $"/api/contentapi/{localPath}";
        return $"{head}{tail}";
    }
}