using System.Collections.Concurrent;
using System.Collections.Specialized;
using JsonNormalize.Models;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.Logic;

public static class JsonNormalizer
{
    private const string RootPath = "root";

    public static JToken Normalize(JToken tok) => Normalize(tok, new());
    
    public static JToken Normalize(JToken tok, NormalizerOptions opt)
    {
        if (!opt.Validate())
            throw new ArgumentException($"{nameof(NormalizerOptions)} is invalid");
	
        return NormalizeReq(tok, opt, RootPath);
    }

    private static JToken NormalizeReq(JToken tok, NormalizerOptions opt, string path)
    {
        if (tok is JArray arr)
            return NormalizeArray(arr, opt, path);
        if (tok is JObject obj)
            return NormalizeObject(obj, opt, path);
        return tok;
    }
    
    private static JArray NormalizeArray(JArray arr, NormalizerOptions opt, string path)
    {
        var items = arr
            .Select((t, i) => (index: i, item: t))
            .OrderByIf(!ShouldKeepOrder(path, opt),
                tpl => opt.ArrayOptions.ArrayItemsEqualityComparer.GetHashCode(tpl.item))
            .ToList();
            
        Parallel.ForEach(items, tpl => NormalizeReq(tpl.item, opt, $"{path}[{tpl.index}]"));
        
        foreach (var (index, item) in items)
        {
            item.Remove();
            arr.Add(item);
        }
        
        return arr;

        bool ShouldKeepOrder(string path, NormalizerOptions opt)
        {
            var trimmedPath = path[(RootPath.Length + 1)..];
            bool? isOrdered = null;
            if (opt.ArrayOptions.OrderedArrayPaths.Contains(trimmedPath)) isOrdered = true;
            else if (opt.ArrayOptions.UnOrdedArrayPaths.Contains(trimmedPath)) isOrdered = false;
            isOrdered ??= opt.ArrayOptions.DefaultArrayOrderCompareBehaviour == ArrayOrderEnum.Ordered;
            
            return isOrdered.Value;
        }
    }
    
    private static JObject NormalizeObject(JObject obj, NormalizerOptions opt, string path)
    {
        var props = obj.Properties()
            .AsEnumerable()
            .OrderByIf(opt.SortObjectsProperties, p => p.Name)
            .ToList();

        Parallel.ForEach(props, prop => NormalizeReq(prop.Value, opt, $"{path}.{prop.Name}"));
        
        foreach (var prop in props)
        {
            prop.Remove();
            obj.Add(prop);
        }

        return obj;
    }
}