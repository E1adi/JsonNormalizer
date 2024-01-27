using JsonNormalize.Models;

namespace JsonNormalize.UnitTests.Models;

[TestFixture]
public class ArrayNormalizationOptionsTests
{
    [Test]
    public void Validate_WhenOrderedAndUnorderedPathsIntersect_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            OrderedCollectionPaths = new HashSet<string> { "a", "b" },
            UnorderedCollectionPaths = new HashSet<string> { "b", "c" }
        };
        
        // Act
        var actual = sut.Validate();
        
        // Assert
        Assert.That(actual, Is.False);
    }

    [Test]
    public void Validate_WhenOrderedAndUnorderedPathsDoNotIntersect_ReturnsTrue()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            OrderedCollectionPaths = new HashSet<string> { "a", "b" },
            UnorderedCollectionPaths = new HashSet<string> { "c", "d" }
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.True);
    }

    [Test]
    public void Validate_WhenOrderedAndUnorderedPathsAreEmpty_ReturnsTrue()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            OrderedCollectionPaths = new HashSet<string>(),
            UnorderedCollectionPaths = new HashSet<string>()
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.True);
    }
    
    [Test]
    public void Validate_WhenUnorderedCollectionPathsAreForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            UnorderedCollectionPaths = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Validate_WhenOrderedCollectionPathsAreForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            OrderedCollectionPaths = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Validate_WhenArrayItemsEqualityComparerIsForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            ArrayItemsEqualityComparer = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Validate_WhenAllPropertiesAreForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            ArrayItemsEqualityComparer = null,
            OrderedCollectionPaths = null,
            UnorderedCollectionPaths = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
}