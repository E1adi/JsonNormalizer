namespace JsonNormalize.Models;

/// <summary>
/// Options for the normalization process.
/// </summary>
public class NormalizerOptions
{
    /// <summary>
    /// Options for normalizing of JSON arrays.
    /// </summary>
    public ArrayNormalizationOptions ArrayOptions { get; set;} = new();
	
    /// <summary>
    /// Specify whether or not to sort the properties of JSON objects.
    /// </summary>
    public bool SortObjectsProperties { get; set; } = true;
    
    /// <summary>
    /// Specify whether or not to parallelize the normalization process.
    /// This can improve normalization of large and complex JSON objects, but for small ones, it can be slower.
    /// </summary>
    public bool ShouldParallelizeProcess { get; set; } = true;
    
    
    /// <summary>
    /// Validates the setting.
    /// </summary>
    /// <returns>whether or not the settings are valid.</returns>
    public bool Validate() =>
        ArrayOptions.Validate();
}