using Newtonsoft.Json.Linq;

namespace JsonNormalize.Models;

public class ArrayNormalizationOptions
{
    public IEqualityComparer<JToken> ArrayItemsEqualityComparer { get; set; } = new JTokenEqualityComparer(); 
    public ArrayOrderEnum DefaultArrayOrderCompareBehaviour { get; init; } = ArrayOrderEnum.Unordered;
    public HashSet<string> OrderedArrayPaths { get; init; } = new();
    public HashSet<string> UnOrdedArrayPaths { get; init; } = new();

    public bool Validate() =>
        ArrayItemsEqualityComparer != null &&
        OrderedArrayPaths != null &&
        UnOrdedArrayPaths != null &&
        !OrderedArrayPaths.Intersect(UnOrdedArrayPaths).Any();
}

public enum ArrayOrderEnum
{
    Unordered,
    Ordered
}