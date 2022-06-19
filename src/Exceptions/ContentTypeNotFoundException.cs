using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    internal class ContentTypeNotFoundException : HalException
    {
        public ContentTypeNotFoundException(string contentTypeAlias)
            : base("content_type_not_found", $"Content type with alias '{contentTypeAlias}' not found")
        {
        }
    }
}