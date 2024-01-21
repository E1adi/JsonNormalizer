using JsonNormalize.Logic;
using JsonNormalize.Models;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.UnitTests.Logic;

[TestFixture]
public class JsonNormalizerTests
{
    [Test]
    public void Normalize_WhenOptionsAreInvalid_ThrowsArgumentException()
    {
        // Arrange
        var opt = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                UnOrdedArrayPaths = null
            }
        };
        
        // Act
        void Act() => JsonNormalizer.Normalize(new JObject(), opt);
        
        // Assert
        Assert.That(Act, Throws.ArgumentException);
    }
    
    [Test]
    public void Normalize_ArrayOrderIsUnimportant_JsonsWouldBeEqual()
    {
        // Arrange
        var opt = new NormalizerOptions();
        var arr1 = new[] { 3, 2, 1 };
        var arr2 = new[] { 4, 5, 6 };
        var json1 = new JObject
        {
            ["a"] = JArray.FromObject(arr1),
            ["b"] = JArray.FromObject(arr2)
        };
        var json2 = new JObject
        {
            ["a"] = JArray.FromObject(arr1.Reverse()),
            ["b"] = JArray.FromObject(arr2.Reverse())
        };
        
        // Act
        var normalized1 = JsonNormalizer.Normalize(json1, opt);
        var normalized2 = JsonNormalizer.Normalize(json2, opt);
        
        // Assert
        Assert.That(normalized1, Is.EqualTo(normalized2));
    }
    
    [Test]
    public void Normalize_ArrayOrderIsImportant_JsonsWouldNotBeEqual()
    {
        // Arrange
        var opt = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                DefaultArrayOrderCompareBehaviour = ArrayOrderEnum.Ordered
            }
        };
        
        var arr1 = new[] { 3, 2, 1 };
        var arr2 = new[] { 4, 5, 6 };
        var json1 = new JObject
        {
            ["a"] = JArray.FromObject(arr1),
            ["b"] = JArray.FromObject(arr2)
        };
        var json2 = new JObject
        {
            ["a"] = JArray.FromObject(arr1.Reverse()),
            ["b"] = JArray.FromObject(arr2.Reverse())
        };
        
        // Act
        var normalized1 = JsonNormalizer.Normalize(json1, opt);
        var normalized2 = JsonNormalizer.Normalize(json2, opt);
        
        // Assert
        Assert.That(normalized1, !Is.EqualTo(normalized2));
    }
        
    [Test]
    public void Normalize_OrderedArrayPathOverrideWhileDefaultIsUnordered_ShouldKeepArrayOrderOnlyForThoseSpecifiedInOverride()
    {
        // Arrange
        var arr1 = new[] { 3, 2, 1 };
        var arr2 = new[] { 6, 5, 4 };
        var json = new JObject
        {
            ["a"] = JArray.FromObject(arr1),
            ["b"] = JArray.FromObject(arr2),
            ["c"] = JObject.FromObject(new
            {
                d = JArray.FromObject(arr2)
            })
        };
        
        var opt = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                DefaultArrayOrderCompareBehaviour = ArrayOrderEnum.Unordered,
                OrderedArrayPaths = new() {"c.d"}
            }
        };

        var comparer = opt.ArrayOptions.ArrayItemsEqualityComparer;
        var arr1Unordered = JArray.FromObject(arr1.OrderBy(i => comparer.GetHashCode(i)).ToArray());
        var arr2Unordered = JArray.FromObject(arr2.OrderBy(i => comparer.GetHashCode(i)).ToArray());
        var arr2Ordered = JArray.FromObject(arr2);
        
        // Act
        var normalized = JsonNormalizer.Normalize(json, opt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(normalized["a"], Is.EquivalentTo(arr1Unordered));
            Assert.That(normalized["b"], Is.EquivalentTo(arr2Unordered));
            Assert.That(normalized["c"]!["d"], Is.EquivalentTo(arr2Ordered));
        });
    }
    
    [Test]
    public void Normalize_UnorderedArrayPathOverrideWhileDefaultIsOrdered_ShouldKeepArrayOrderOnlyForAllExceptThoseSpecifiedInOverride()
    {
        // Arrange
        var arr1 = new[] { 3, 2, 1 };
        var arr2 = new[] { 6, 5, 4 };
        var json = new JObject
        {
            ["a"] = JArray.FromObject(arr1),
            ["b"] = JArray.FromObject(arr2),
            ["c"] = JObject.FromObject(new
            {
                d = JArray.FromObject(arr2)
            })
        };
        
        var opt = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                DefaultArrayOrderCompareBehaviour = ArrayOrderEnum.Ordered,
                UnOrdedArrayPaths = new() {"c.d"}
            }
        };

        var comparer = opt.ArrayOptions.ArrayItemsEqualityComparer;
        var arr1Ordered = JArray.FromObject(arr1);
        var arr2Ordered = JArray.FromObject(arr2);
        var arr2Unordered = JArray.FromObject(arr2.OrderBy(i => comparer.GetHashCode(i)).ToArray());
        
        // Act
        var normalized = JsonNormalizer.Normalize(json, opt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(normalized["a"], Is.EquivalentTo(arr1Ordered));
            Assert.That(normalized["b"], Is.EquivalentTo(arr2Ordered));
            Assert.That(normalized["c"]["d"], Is.EquivalentTo(arr2Unordered));
        });
    }
    
    // Test for cases where the paths doesn't exists in the json
    
    [Test]
    public void Normalize_WhenDefaultIsOrderedAndOverridePathPointToNonExistentArray_ShouldReturnSameJson()
    {
        // Arrange
        var opt = new NormalizerOptions
        {
            ArrayOptions = new()
            {
                DefaultArrayOrderCompareBehaviour = ArrayOrderEnum.Ordered,
                UnOrdedArrayPaths = new() { "c" }
            }
        };
        
        var json = new JObject
        {
            ["a"] = JArray.FromObject(new[] { 1, 2, 3 }),
            ["b"] = JArray.FromObject(new[] { 6, 5, 4 })
        };
        
        // Act
        var normalized = JsonNormalizer.Normalize(json, opt);

        // Assert
        Assert.That(normalized, Is.EquivalentTo(json));
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void Normalize_TwoComplexJsons_ShouldBeEquivalent(bool shouldOrderProperties)
    {
        // Arrange
        var opt = new NormalizerOptions { SortObjectsProperties = shouldOrderProperties };
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));
        var comparer = new JTokenEqualityComparer();
        
        // Act
        var normalized1 = JsonNormalizer.Normalize(json1, opt);
        var normalized2 = JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(comparer.Equals(normalized1, normalized2), Is.True);
    }
    
    [Test]
    public void Normalize_ToStringCompare_ShouldBeEquivalent()
    {
        // Arrange
        var opt = new NormalizerOptions { SortObjectsProperties = true };
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));
        
        // Act
        var normalized1 = JsonNormalizer.Normalize(json1, opt).ToString();
        var normalized2 = JsonNormalizer.Normalize(json2, opt).ToString();

        // Assert
        Assert.That(normalized1, Is.EqualTo(normalized2));
    }
    
    [Test]
    public void Normalize_PropertiesSorted_CompareUsingEquals_ShouldBeEquivalent()
    {
        // Arrange
        var opt = new NormalizerOptions { SortObjectsProperties = true };
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));
        
        // Act
        var normalized1 = JsonNormalizer.Normalize(json1, opt);
        var normalized2 = JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(normalized1, Is.EqualTo(normalized2));
    }
}