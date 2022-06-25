using System;

namespace Flaeng.Umbraco.ContentAPI;

[Serializable]
public abstract class HalException : Exception
{
    public string SystemMessage { get; init; }

    public HalException(string SystemMessage, string Message)
        : base(Message)
    {
        this.SystemMessage = SystemMessage;
    }
}
