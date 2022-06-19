using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    public class UnknownPropertyException : HalException
    {
        public string ContentTypeAlias { get; init; }
        public string PropertyAlias { get; init; }

        public UnknownPropertyException(string ContentTypeAlias, string PropertyAlias)
            : base("unknown_property", $"Failed to find property with alias '{PropertyAlias}' on content type with alias '{ContentTypeAlias}'")
        {
            this.ContentTypeAlias = ContentTypeAlias;
            this.PropertyAlias = PropertyAlias;
        }
    }
}