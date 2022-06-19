using System;
using System.Runtime.Serialization;

namespace Flaeng.Umbraco.ContentAPI
{
    [Serializable]
    public class UnknownOperatorException : HalException
    {
        public string Operator { get; init; }

        public UnknownOperatorException(string Operator)
            : base("unknown_operator", $"Unknown operator '{Operator}'")
        {
            this.Operator = Operator;
        }
    }
}