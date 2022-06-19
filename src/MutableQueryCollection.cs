using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Flaeng.Umbraco.ContentAPI;

public class MutableQueryCollection : Dictionary<string, StringValues>, IQueryCollection
{
    ICollection<string> IQueryCollection.Keys => base.Keys;

    public MutableQueryCollection()
    {
    }

    public MutableQueryCollection(string query)
    {
        foreach (var item in query.Split('&'))
        {
            var itemSplit = item.Split('=');
            Add(itemSplit[0], String.Join("=", itemSplit.Skip(1)));
        }
    }

    public MutableQueryCollection(IEnumerable<KeyValuePair<string, string>> collection)
    {
        foreach (var item in collection)
            Add(item.Key, new StringValues(item.Value));
    }

    public MutableQueryCollection(IEnumerable<KeyValuePair<string, StringValues>> collection)
    {
        foreach (var item in collection)
            Add(item.Key, item.Value);
    }

    public MutableQueryCollection Copy() => new MutableQueryCollection(this);

    public override string ToString() => String.Join("&", this.Select(x => $"{x.Key}={x.Value}"));
}