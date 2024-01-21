using Newtonsoft.Json.Linq;

namespace JsonNormalize.Logic;

public static class Utils
{
    public static JObject ToJObject(this IEnumerable<KeyValuePair<string, JToken>> source)
    {
        var obj = new JObject();
        foreach (var prop in source)
        {
            obj[prop.Key] = prop.Value;
        }

        return obj;
    }
    
    public static JArray ToJArray(this IEnumerable<JToken> source)
    {
        var arr = new JArray();
        foreach (var prop in source)
        {
            arr.Add(prop);
        }

        return arr;
    }
}