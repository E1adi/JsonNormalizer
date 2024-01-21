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
        var concurrentDict = new ConcurrentDictionary<int, JToken>();
        Parallel.ForEach(arr.Select((t, i) => (i, t)), tpl => 
        {
            var (index, tok) = tpl;
            concurrentDict[index] = NormalizeReq(tok, opt, $"{path}[{index}]");
        });
	
        var ordered = ShouldKeepOrder(path, opt) ?
            concurrentDict.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value) :
            concurrentDict.Select(kvp => kvp.Value).OrderBy(t =>  opt.ArrayOptions.ArrayItemsEqualityComparer.GetHashCode(t)); 
        
        return ordered.ToJArray();

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
        var normalized = new ConcurrentDictionary<string, JToken>();
        Parallel.ForEach(obj.Properties().OrderBy(p => p.Name), prop =>
        {
            normalized[prop.Name] = NormalizeReq(prop.Value, opt, $"{path}.{prop.Name}");
        });
        
        return opt.SortObjectsProperties ? 
            normalized.OrderBy(kvp => kvp.Key).ToJObject() :
            normalized.ToJObject();
    }
}