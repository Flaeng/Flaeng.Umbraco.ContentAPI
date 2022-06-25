
using System;

namespace Flaeng.Umbraco.ContentAPI;

[Serializable]
public class InvalidQueryParameterException : HalException
{
    public InvalidQueryParameterException(string queryParameter)
        : base("invalid_query_parameter", $"Query parameter value with key '{queryParameter}' was invalid")
    {
    }
}