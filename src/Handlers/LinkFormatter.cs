using System;
using System.Linq;

using Flaeng.Umbraco.ContentAPI.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Flaeng.Umbraco.ContentAPI.Handlers;

public interface ILinkFormatter
{
    string FormatHref(string localPath, string[] expand);
}
public static class ILinkFormatterExtensions
{
    public static string FormatHref(this ILinkFormatter formatter, string localPath)
        => formatter.FormatHref(localPath, new string[0]);
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

    public virtual string FormatHref(string localPath, string[] expand)
    {
        var head = options.PrependAbsolutePathToLinks ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}" : String.Empty;
        var body = String.IsNullOrWhiteSpace(localPath) ? "/api/contentapi" : $"/api/contentapi/{localPath}";

        var expandCompact = expand.Where(x => !expand.Any(z => z.StartsWith($"{x}."))).ToArray();
        var tail = expand.Length == 0 ? String.Empty : $"?expand={String.Join(",", expandCompact)}";
        return $"{head}{body}{tail}";
    }
}