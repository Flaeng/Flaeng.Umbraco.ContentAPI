using System;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    public class InvalidFilterException : HalException
    {
        public string FilterText { get; init; }

        public InvalidFilterException(string FilterText)
            : base("invalid_filter", $"Invalid filter: {FilterText}")
        {
            this.FilterText = FilterText;
        }
    }
}