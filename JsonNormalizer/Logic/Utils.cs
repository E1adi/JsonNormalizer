using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.Logic;

public static class Utils
{
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }
    
    public static IEnumerable<TSource> OrderByIf<TSource, TKey>(
        this IEnumerable<TSource> source, bool condition, Func<TSource, TKey> comparer) =>
        condition ? source.OrderBy(comparer) : source;
}