using System;
using System.Collections.Generic;

namespace SugarTalk.Core.Extensions;

public static class EnumerableExtension
{
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> callback)
    {
        foreach (var item in items)
        {
            callback(item);
        }
    }
}