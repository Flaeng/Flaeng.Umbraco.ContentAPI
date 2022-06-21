
using System;

namespace Flaeng.Umbraco.ContentAPI.Options;

public interface IAllowCrudOperation : IContentApiOptions
{
    IAllowCrudOperation If(Func<ICrudRequest, bool> condition);
}