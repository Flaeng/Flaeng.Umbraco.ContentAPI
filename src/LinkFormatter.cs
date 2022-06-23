using System;

namespace Flaeng.Umbraco.ContentAPI;

public interface ILinkFormatter
{
    string FormatHref(string localPath);
}
public class DefaultLinkFormatter : ILinkFormatter
{
    public string FormatHref(string localPath)
        => String.IsNullOrWhiteSpace(localPath) ? "/api/contentapi" : $"/api/contentapi/{localPath}";
}