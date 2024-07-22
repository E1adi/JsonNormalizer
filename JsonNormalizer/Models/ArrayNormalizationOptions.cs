using JsonNormalize.Logic;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.Models;

/// <summary>
/// Used to configure normalization of JSON arrays.
/// </summary>
public class ArrayNormalizationOptions
{
    /// <summary>
    /// The comparer used to compare array items when we want to normalize 
    /// an order agnostic collection that is represented by an array.
    /// </summary>
    public IEqualityComparer<JToken> ArrayItemsEqualityComparer { get; set; } = new JTokenEqualityComparer();
    
    /// <summary>
    /// How would the normalizer treat JSON arrays
    /// Do they represent an ordered or unordered collection 
    /// </summary>
    public CollectionOrderSpec DefaultCollectionOrderSpec { get; init; } = CollectionOrderSpec.Unordered;
    
    /// <summary>
    /// Paths of <b>Ordered</b> collections.
    /// This will override <see cref="DefaultCollectionOrderSpec"/> setting for paths specified here.
    /// </summary>
    public HashSet<string> OrderedCollectionPaths { get; init; } = new();
    
    /// <summary>
    /// Paths of <b>Unordered</b> collections.
    /// This will override <see cref="DefaultCollectionOrderSpec"/> setting for paths specified here.
    /// </summary>
    public HashSet<string> UnorderedCollectionPaths { get; init; } = new();

    
    internal Lazy<IEnumerable<WildcardPathMatcher>> OrderedCollectionPathMatchers => new(() =>
        OrderedCollectionPaths.Select(path => new WildcardPathMatcher(path)));
    
    internal Lazy<IEnumerable<WildcardPathMatcher>> UnorderedCollectionPathMatchers => new(() =>
        UnorderedCollectionPaths.Select(path => new WildcardPathMatcher(path)));
    
    /// <summary>
    ///  Validates the setting.
    /// </summary>
    /// <returns> Whether the settings are valid.</returns>
    public bool Validate() =>
        ArrayItemsEqualityComparer != null &&
        OrderedCollectionPaths != null &&
        UnorderedCollectionPaths != null &&
        !OrderedCollectionPaths.Intersect(UnorderedCollectionPaths).Any();
}

/// <summary>
/// Used to specify the type of a collection in terms of order importance.
/// </summary>
public enum CollectionOrderSpec
{
    /// <summary>
    /// Specify it is an <b>Unordered</b> collection
    /// </summary>
    Unordered,
    
    /// <summary>
    /// Specify it is an <b>Ordered</b> collection
    /// </summary>
    Ordered
}