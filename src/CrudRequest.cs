using Umbraco.Cms.Core.Models.PublishedContent;

namespace Flaeng.Umbraco.ContentAPI;

public interface ICrudRequest
{
    IPublishedContent Content { get; }
}
public class CrudRequest : ICrudRequest
{
    public IPublishedContent Content { get; init; }
}