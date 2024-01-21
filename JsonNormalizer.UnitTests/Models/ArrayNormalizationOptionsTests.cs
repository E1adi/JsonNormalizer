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
            OrderedArrayPaths = new HashSet<string> { "a", "b" },
            UnOrdedArrayPaths = new HashSet<string> { "b", "c" }
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
            OrderedArrayPaths = new HashSet<string> { "a", "b" },
            UnOrdedArrayPaths = new HashSet<string> { "c", "d" }
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
            OrderedArrayPaths = new HashSet<string>(),
            UnOrdedArrayPaths = new HashSet<string>()
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.True);
    }
    
    [Test]
    public void Validate_WhenUnorderedArrayPathsAreForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            UnOrdedArrayPaths = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void Validate_WhenOrderedArrayPathsAreForcedNull_ReturnsFalse()
    {
        // Arrange
        var sut = new ArrayNormalizationOptions
        {
            OrderedArrayPaths = null
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
            OrderedArrayPaths = null,
            UnOrdedArrayPaths = null
        };

        // Act
        var actual = sut.Validate();

        // Assert
        Assert.That(actual, Is.False);
    }
}