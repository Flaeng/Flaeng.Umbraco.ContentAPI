using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Flaeng.Umbraco.Extensions;

public static class QueryCollectionExtensions
{
    public static string GetValue(this IQueryCollection collection, string key)
        => collection.TryGetValue(key, out var value) ? value : default;

    public static string GetValue(this IQueryCollection collection, string key, StringValues defaultValue)
        => collection.TryGetValue(key, out var value) ? value : defaultValue;
}