using JsonNormalize.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.Logic;

public static class JsonNormalizer
{
    private const string RootPath = "root";

    /// <summary>
    /// Parse and Normalize a JSON represented by a <see cref="string"/> using the default options for <see cref="NormalizerOptions"/>.
    /// </summary>
    /// <param name="json"><see cref="string"/> representation of a JSON you wish to normalize</param>
    /// <returns><see cref="JToken"/> loaded with the normalized version of the JSON</returns>
    public static JToken Normalize(string json) => 
        Normalize(json, new());


    /// <summary>
    /// Parse and Normalize a JSON represented by a <see cref="string"/> using the specified <see cref="NormalizerOptions"/>.
    /// </summary>
    /// <param name="json"><see cref="string"/> representation of a JSON you wish to normalize</param>
    /// <param name="opt">The options with which you want to Normalize the JSON</param>
    /// <returns><see cref="JToken"/> loaded with the normalized version of the JSON</returns>
    /// <exception cref="ArgumentNullException">One of the arguments is null</exception>
    /// <exception cref="ArgumentException">Invalid json representation or invalid <see cref="NormalizerOptions"/></exception>
    public static JToken Normalize(string json, NormalizerOptions opt)
    {
        if (json == null)
            throw new ArgumentNullException(nameof(json));
        
        if (opt == null)
            throw new ArgumentNullException(nameof(opt));
        
        if (!TryParse(json, out var tok))
            throw new ArgumentException("Invalid JSON");
                
        if (!opt.Validate())
            throw new ArgumentException($"{nameof(NormalizerOptions)} is invalid");
        
        return NormalizeReq(tok!, opt, RootPath);
    }

    /// <summary>
    /// Normalize <b>in-place</b> a <see cref="JToken"/> using the default options for <see cref="NormalizerOptions"/>.
    /// </summary>
    /// <param name="tok"><see cref="JToken"/> you wish to normalize</param>
    public static void Normalize(JToken tok) => Normalize(tok, new());
    
    /// <summary>
    /// Normalize <b>in-place</b> a <see cref="JToken"/> using the specified <see cref="NormalizerOptions"/>.
    /// </summary>
    /// <param name="tok"><see cref="JToken"/> you wish to normalize</param>
    /// <param name="opt">The options with which you want to Normalize the JSON</param>
    /// <exception cref="ArgumentNullException">One of the arguments is null</exception>
    /// <exception cref="ArgumentException">Invalid <see cref="NormalizerOptions"/></exception>
    public static void Normalize(JToken tok, NormalizerOptions opt)
    {
        if (opt == null)
            throw new ArgumentNullException(nameof(opt));
        
        if (tok == null)
            throw new ArgumentNullException(nameof(tok));
        
        if (!opt.Validate())
            throw new ArgumentException($"{nameof(NormalizerOptions)} is invalid");
	
        NormalizeReq(tok, opt, RootPath);
    }
    
    private static bool TryParse(string json, out JToken? token)
    {
        token = null;
        try
        {
            token = JToken.Parse(json);
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
    }

    private static JToken NormalizeReq(JToken tok, NormalizerOptions opt, string path)
    {
        var isAsync = opt.ShouldParallelizeProcess;
        return tok switch
        {
            JArray arr => isAsync ? NormalizeArrayAsync(arr, opt, path) : NormalizeArray(arr, opt, path),
            JObject obj => isAsync ? NormalizeObjectAsync(obj, opt, path) : NormalizeObject(obj, opt, path),
            _ => tok
        };
    }

    private static JObject NormalizeObject(JObject obj, NormalizerOptions opt, string path)
    {
        var props = GetOrderedProperties(obj, opt);
        
        foreach (var prop in props)
        {
            prop.Remove();
            NormalizeReq(prop.Value, opt, $"{path}.{prop.Name}");
            obj.Add(prop);
        }

        return obj;
    }
    
    private static JObject NormalizeObjectAsync(JObject obj, NormalizerOptions opt, string path)
    {
        var props = GetOrderedProperties(obj, opt);

        Parallel.ForEach(props, prop => NormalizeReq(prop.Value, opt, $"{path}.{prop.Name}"));
        
        foreach (var prop in props)
        {
            prop.Remove();
            obj.Add(prop);
        }

        return obj;
    }

    private static IEnumerable<JProperty> GetOrderedProperties(JObject obj, NormalizerOptions opt) => 
        obj.Properties()
            .OrderByIf(opt.SortObjectsProperties, p => p.Name)
            .ToList();

    private static JArray NormalizeArray(JArray arr, NormalizerOptions opt, string path)
    {
        var items = GetOrderedItemsWithInitialIndex(arr, opt, path);
            
        foreach (var (index, item) in items)
        {
            item.Remove();
            NormalizeReq(item, opt, $"{path}[{index}]");
            arr.Add(item);
        }
        
        return arr;
    }

    private static JArray NormalizeArrayAsync(JArray arr, NormalizerOptions opt, string path)
    {
        var items = GetOrderedItemsWithInitialIndex(arr, opt, path);
            
        Parallel.ForEach(items, tpl => NormalizeReq(tpl.Item, opt, $"{path}[{tpl.Index}]"));
        
        foreach (var (_, item) in items)
        {
            item.Remove();
            arr.Add(item);
        }
        
        return arr;
    }
    
    private static IEnumerable<(int Index, JToken Item)> GetOrderedItemsWithInitialIndex(JArray arr, NormalizerOptions opt, string path) =>
        arr.Select((t, i) => (Index: i, Item: t))
            .OrderByIf(!ShouldKeepOrder(path, opt),
                tpl => opt.ArrayOptions.ArrayItemsEqualityComparer.GetHashCode(tpl.Item))
            .ToList();
    
    private static bool ShouldKeepOrder(string path, NormalizerOptions opt)
    {
        var trimmedPath = path[(RootPath.Length + 1)..];
        bool? isOrdered = null;
        if (opt.ArrayOptions.OrderedCollectionPaths.Contains(trimmedPath)) isOrdered = true;
        else if (opt.ArrayOptions.UnorderedCollectionPaths.Contains(trimmedPath)) isOrdered = false;
        isOrdered ??= opt.ArrayOptions.DefaultCollectionOrderSpec == CollectionOrderSpec.Ordered;
            
        return isOrdered.Value;
    }
}