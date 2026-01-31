namespace Budgetr.Shared.Models;

/// <summary>
/// Represents a configurable meter that can be activated to track time.
/// </summary>
public class Meter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Display name for the meter (e.g., "+1x", "-1x", "+1.5x").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The multiplier for this meter. Positive values add time, negative subtract.
    /// </summary>
    public double Factor { get; set; }
    
    /// <summary>
    /// Order in which this meter appears in the UI.
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Creates a formatted display name based on the factor.
    /// </summary>
    public static string FormatFactorName(double factor)
    {
        var sign = factor >= 0 ? "+" : "";
        return $"{sign}{factor}x";
    }
}
