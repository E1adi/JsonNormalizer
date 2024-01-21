namespace JsonNormalize.Models;

public class NormalizerOptions
{
    public ArrayNormalizationOptions ArrayOptions { get; set;} = new();
	
    public bool SortObjectsProperties { get; set; } = true;
    
    public bool Validate() =>
        ArrayOptions.Validate();
}