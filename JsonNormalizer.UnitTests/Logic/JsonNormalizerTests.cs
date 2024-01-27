using JsonNormalize.Logic;
using JsonNormalize.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonNormalize.UnitTests.Logic;

[TestFixture]
public class JsonNormalizerTests
{
    [Test]
    public void Normalize_WhenOptionsIsNull_ThrowsArgumentNullException()
    {
        // Act
        void Act() => JsonNormalizer.Normalize(new JObject(), null);

        // Assert
        Assert.That(Act, Throws.ArgumentNullException);
    }
    
    [Test]
    public void Normalize_WhenOptionsAreInvalid_ThrowsArgumentException()
    {
        // Arrange
        var opt = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions { UnorderedCollectionPaths = null }
        };

        // Act
        void Act() => JsonNormalizer.Normalize(new JObject(), opt);

        // Assert
        Assert.That(Act, Throws.ArgumentException);
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void Normalize_CollectionOrderIsUnimportant_JsonsWouldBeEqual(NormalizerOptions opt)
    {
        // Arrange
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
        JsonNormalizer.Normalize(json1, opt);
        JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(json1, Is.EqualTo(json2));
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void Normalize_CollectionOrderIsImportant_JsonsWouldNotBeEqual(NormalizerOptions opt)
    {
        // Arrange
        opt.ArrayOptions = new ArrayNormalizationOptions { DefaultCollectionOrderSpec = CollectionOrderSpec.Ordered };

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
        JsonNormalizer.Normalize(json1, opt);
        JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(json1, !Is.EqualTo(json2));
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void
        Normalize_OrderedCollectionPathsWhileDefaultIsUnordered_ShouldKeepCollectionsOrderOnlyForThoseSpecifiedInOverride(
            NormalizerOptions opt)
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

        opt.ArrayOptions = new ArrayNormalizationOptions
        {
            DefaultCollectionOrderSpec = CollectionOrderSpec.Unordered,
            OrderedCollectionPaths = new() { "c.d" }
        };

        var comparer = opt.ArrayOptions.ArrayItemsEqualityComparer;
        var arr1Unordered = JArray.FromObject(arr1.OrderBy(i => comparer.GetHashCode(i)).ToArray());
        var arr2Unordered = JArray.FromObject(arr2.OrderBy(i => comparer.GetHashCode(i)).ToArray());
        var arr2Ordered = JArray.FromObject(arr2);

        // Act
        JsonNormalizer.Normalize(json, opt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(json["a"], Is.EquivalentTo(arr1Unordered));
            Assert.That(json["b"], Is.EquivalentTo(arr2Unordered));
            Assert.That(json["c"]!["d"], Is.EquivalentTo(arr2Ordered));
        });
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void
        Normalize_UUnorderedCollectionPathsWhileDefaultIsOrdered_ShouldKeepCollectionOrderOnlyForAllExceptThoseSpecifiedInOverride(
            NormalizerOptions opt)
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

        opt.ArrayOptions = new ArrayNormalizationOptions
        {
            DefaultCollectionOrderSpec = CollectionOrderSpec.Ordered,
            UnorderedCollectionPaths = new() { "c.d" }
        };

        var comparer = opt.ArrayOptions.ArrayItemsEqualityComparer;
        var arr1Ordered = JArray.FromObject(arr1);
        var arr2Ordered = JArray.FromObject(arr2);
        var arr2Unordered = JArray.FromObject(arr2.OrderBy(i => comparer.GetHashCode(i)).ToArray());

        // Act
        JsonNormalizer.Normalize(json, opt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(json["a"], Is.EquivalentTo(arr1Ordered));
            Assert.That(json["b"], Is.EquivalentTo(arr2Ordered));
            Assert.That(json["c"]!["d"], Is.EquivalentTo(arr2Unordered));
        });
    }

    // Test for cases where the paths doesn't exists in the json

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void Normalize_WhenDefaultIsOrderedAndOverridePathPointToNonExistentCollection_ShouldReturnSameJson(
        NormalizerOptions opt)
    {
        // Arrange
        opt.ArrayOptions = new()
        {
            DefaultCollectionOrderSpec = CollectionOrderSpec.Ordered,
            UnorderedCollectionPaths = new() { "c" }
        };

        var json = new JObject
        {
            ["a"] = JArray.FromObject(new[] { 1, 2, 3 }),
            ["b"] = JArray.FromObject(new[] { 6, 5, 4 })
        };

        // Act
        var normalized = JsonNormalizer.Normalize(json.ToString(Formatting.None), opt);

        // Assert
        Assert.That(normalized, Is.EquivalentTo(json));
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptionsWithShouldOrderProperties), new object[] {true})]
    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptionsWithShouldOrderProperties), new object[] {false})]
    public void Normalize_TwoComplexJsons_ShouldBeEquivalent(NormalizerOptions opt)
    {
        // Arrange
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));
        var comparer = new JTokenEqualityComparer();
        Assert.That(comparer.Equals(json1, json2), Is.False);

        // Act
        JsonNormalizer.Normalize(json1, opt);
        JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(comparer.Equals(json1, json2), Is.True);
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void Normalize_ToStringCompare_ShouldBeEquivalent(NormalizerOptions opt)
    {
        // Arrange
        opt.SortObjectsProperties = true;
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));

        // Act
        JsonNormalizer.Normalize(json1, opt);
        JsonNormalizer.Normalize(json2, opt);
        var normalized1 = json1.ToString(Formatting.None);
        var normalized2 = json2.ToString(Formatting.None);

        // Assert
        Assert.That(normalized1, Is.EqualTo(normalized2));
    }

    [TestCaseSource(nameof(GetBothSynchronousAndAsynchronousOptions))]
    public void Normalize_PropertiesSorted_CompareUsingEquals_ShouldBeEquivalent(NormalizerOptions opt )
    {
        // Arrange
        opt.SortObjectsProperties = true;
        var json1 = JObject.Parse(File.ReadAllText("TestData/ComplexJson.json"));
        var json2 = JObject.Parse(File.ReadAllText("TestData/ComplexJson-Scrambled.json"));
        Assert.That(json1, !Is.EqualTo(json2));
        
        // Act
        JsonNormalizer.Normalize(json1, opt);
        JsonNormalizer.Normalize(json2, opt);

        // Assert
        Assert.That(json1, Is.EqualTo(json2));
    }

    public static IEnumerable<NormalizerOptions> GetBothSynchronousAndAsynchronousOptions()
    {
        yield return new NormalizerOptions { ShouldParallelizeProcess = false };
        yield return new NormalizerOptions { ShouldParallelizeProcess = true };
    }
    
    public static IEnumerable<NormalizerOptions> GetBothSynchronousAndAsynchronousOptionsWithShouldOrderProperties(
        bool? shouldSortProperties = null)
    {
        var defaultOptions = new NormalizerOptions();
        var sortProperties = shouldSortProperties ?? defaultOptions.SortObjectsProperties;
        return GetBothSynchronousAndAsynchronousOptions()
            .Do(opt => opt.SortObjectsProperties = sortProperties);
    }
}