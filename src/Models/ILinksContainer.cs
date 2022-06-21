using System.Collections.Generic;

namespace Flaeng.Umbraco.ContentAPI.Models;

public interface ILinksContainer
{
    Dictionary<string, HalObject> Links { get; }
}