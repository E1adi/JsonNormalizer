namespace JsonNormalize.Logic;

public class WildcardPathMatcher(string path)
{
    private const string WildCard = "*";
    private readonly IReadOnlyList<string> _definitionSegments = path.Split(".");
    
    
    public bool Matches(string actualPath)
    {
        var actualSegments = actualPath.Split('.');
        if (_definitionSegments.Count > actualSegments.Length) 
            return false; // Definition is longer than actual path
        
        var actualIndex = 0;
        var defIndex = 0;
        for (; defIndex < _definitionSegments.Count; defIndex++)
        {
            if (IsWildCard())
            {
                // Skip consecutive wildcards
                while (HasNextSegment() && NextSegmentIsWildcard()) defIndex++;
                
                // Wildcard at the end matches any remaining path
                if (!HasNextSegment())
                    return true;
                
                var nextSegment = GetNextSegment();
                
                var found = false;
                while (actualIndex < actualSegments.Length)
                {
                    if (actualSegments[actualIndex] == nextSegment)
                    {
                        found = true;
                        break;
                    }
                    
                    actualIndex++;
                }
                
                if (!found) return false; // Next segment not found after wildcard
                defIndex++; // Skip next segment in definition since it's matched
            }
            else
            {
                if (actualIndex >= actualSegments.Length || 
                    actualSegments[actualIndex] != GetCurrentSegment())
                    return false; // Mismatch found or actual path ended before definition
            }
            
            actualIndex++;
        }

        return actualIndex == actualSegments.Length; // Ensure all actual segments were matched

        string GetNextSegment() => _definitionSegments[defIndex + 1];
        string GetCurrentSegment() => _definitionSegments[defIndex];
        bool IsWildCard() => _definitionSegments[defIndex] == WildCard;
        bool HasNextSegment() => defIndex < _definitionSegments.Count - 1;
        bool NextSegmentIsWildcard() => _definitionSegments[defIndex + 1] == WildCard;
    }
}