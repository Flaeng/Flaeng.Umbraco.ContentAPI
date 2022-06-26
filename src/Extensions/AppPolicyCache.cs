using System;

using Umbraco.Cms.Core.Cache;

namespace Flaeng.Umbraco.Extensions;

public static class AppPolicyCacheExtensions
{
    public static T Get<T>(
        this IAppPolicyCache cache, 
        string key, 
        Func<T> factory, 
        TimeSpan? timeout, 
        bool isSliding) where T : class
    {
        var result = cache.Get(key, () => factory(), timeout, isSliding);
        return result as T;
    }
}