namespace Flaeng.Umbraco.ContentAPI;

public interface ILinkFormatter
{
    string FormatHref(string localPath);
}
public class DefaultLinkFormatter : ILinkFormatter
{
    public string FormatHref(string localPath) => $"/api/contentapi/{localPath}";
}
