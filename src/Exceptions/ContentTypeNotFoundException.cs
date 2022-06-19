using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    public class ContentTypeNotFoundException : HalException
    {
        public string ContentTypeAlias { get; init; }
        
        public ContentTypeNotFoundException(string ContentTypeAlias)
            : base("content_type_not_found", $"Content type with alias '{ContentTypeAlias}' not found")
        {
            this.ContentTypeAlias = ContentTypeAlias;
        }
    }
}