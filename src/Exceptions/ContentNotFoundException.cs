using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    internal class ContentNotFoundException : HalException
    {
        private string contentType;
        private string contentId;

        public ContentNotFoundException(string contentTypeAlias, string contentId)
            : base("content_not_found", $"Content with id '{contentId}' and content type alias '{contentTypeAlias}' not found")
        {
            this.contentType = contentTypeAlias;
            this.contentId = contentId;
        }
    }
}