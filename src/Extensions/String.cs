using System;

namespace Flaeng.Umbraco.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(this string str)
        => str.Length == 0 ? str : Char.ToLower(str[0]) + String.Join(String.Empty, str.Substring(1).Replace(" ", ""));
}