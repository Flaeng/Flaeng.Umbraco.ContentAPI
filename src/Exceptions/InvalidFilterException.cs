using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    internal class InvalidFilterException : HalException
    {
        public InvalidFilterException(string filterText)
            : base("invalid_filter", $"Invalid filter: {filterText}")
        {
        }
    }
}