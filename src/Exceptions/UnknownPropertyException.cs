using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    internal class UnknownPropertyException : HalException
    {
        public UnknownPropertyException(string contentTypeAlias, string propertyAlias)
            : base("unknown_property", $"Failed to find property with alias '{propertyAlias}' on content type with alias '{contentTypeAlias}'")
        {
        }
    }
}