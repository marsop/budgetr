namespace Budgetr.Shared.Models;

/// <summary>
/// Represents the user's time tracking account state.
/// </summary>
public class TimeAccount
{
    /// <summary>
    /// All meter events (historical and current).
    /// </summary>
    public List<MeterEvent> Events { get; set; } = new();
    
    /// <summary>
    /// Configured meters available to the user.
    /// </summary>
    public List<Meter> Meters { get; set; } = new();
    
    /// <summary>
    /// Gets the currently active event, if any.
    /// </summary>
    public MeterEvent? ActiveEvent => Events.FirstOrDefault(e => e.IsActive);
    
    /// <summary>
    /// Calculates the current total balance from all events.
    /// </summary>
    public TimeSpan CurrentBalance => TimeSpan.FromTicks(
        Events.Sum(e => e.TimeContribution.Ticks)
    );
    
    /// <summary>
    /// Creates a default account with +1x and -1x meters.
    /// </summary>
    public static TimeAccount CreateDefault()
    {
        return new TimeAccount
        {
            Meters = new List<Meter>
            {
                new Meter { Name = "+1x", Factor = 1.0, DisplayOrder = 0 },
                new Meter { Name = "-1x", Factor = -1.0, DisplayOrder = 1 }
            }
        };
    }
}
