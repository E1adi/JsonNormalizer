using System.Security;
using JsonNormalize.Logic;

namespace JsonNormalize.UnitTests.Logic;

[TestFixture]
public class WildcardPathMatcherTests
{
    [TestCase("A.B.C", "A.B.C")]
    [TestCase("A.*.D", "A.B.C.D")]
    [TestCase("*", "A.B.C.D")]
    [TestCase("*.*.*", "A.B.C.D")]
    [TestCase("A.B.*", "A.B.C.D")]
    [TestCase("*.C.D", "A.B.C.D")]
    public void Matches_PositiveChecks_ShouldReturnTrue(string pattern, string actualPath)
    {
        // Arrange
        var pathMatcher = new WildcardPathMatcher(pattern);
        
        // Act
        var result = pathMatcher.Matches(actualPath);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestCase("A.B", "A.B.C", Description = "Pattern with no wildcards and different than actual path")]
    [TestCase("A.*.C.D", "A.B.C.E", Description = "Pattern dictate last char should be 'D' but actual path ends with 'E'")]
    [TestCase("A.*.*.D", "A.B.D", Description = "Pattern has more levels than actual path")]
    [TestCase("A.*.C.D", "A.B.C", Description = "Pattern has more levels than actual path")]
    [TestCase("*.*.*.*.*", "A.B.D.C", Description = "Pattern has more levels than actual path")]
    public void Matches_NegativeChecks_ShouldReturnFalse(string pattern, string actualPath)
    {
        // Arrange
        var pathMatcher = new WildcardPathMatcher(pattern);
        
        // Act
        var result = pathMatcher.Matches(actualPath);
        
        // Assert
        Assert.IsFalse(result);
    }
}