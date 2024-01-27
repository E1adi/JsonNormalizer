using JsonNormalize.Models;

namespace JsonNormalize.UnitTests.Models;

[TestFixture]
public class NormalizerOptionsTests
{
    [Test]
    public void Validate_WhenArrayOptionsAreValid_ReturnsTrue()
    {
        // Arrange
        var sut = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                OrderedCollectionPaths = new HashSet<string> { "a", "b" },
                UnorderedCollectionPaths = new HashSet<string> { "c", "d" }
            }
        };
        
        // Act
        var actual = sut.Validate();
        
        // Assert
        Assert.That(actual, Is.True);
    }
    
    [Test]
    public void Validate_WhenArrayOptionsAreInvalid_ReturnsFalse()
    {
        // Arrange
        var sut = new NormalizerOptions
        {
            ArrayOptions = new ArrayNormalizationOptions
            {
                OrderedCollectionPaths = new HashSet<string> { "a", "b" },
                UnorderedCollectionPaths = new HashSet<string> { "b", "c" }
            }
        };
        
        // Act
        var actual = sut.Validate();
        
        // Assert
        Assert.That(actual, Is.False);
    }
}