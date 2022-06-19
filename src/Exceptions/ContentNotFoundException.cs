using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    public class ContentNotFoundException : HalException
    {
        public string ContentType { get; init; }
        public string ContentId { get; init; }

        public ContentNotFoundException(string contentTypeAlias, string contentId)
            : base("content_not_found", $"Content with id '{contentId}' and content type alias '{contentTypeAlias}' not found")
        {
            this.ContentType = contentTypeAlias;
            this.ContentId = contentId;
        }
    }
}