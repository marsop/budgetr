using Budgetr.Shared.Models;

namespace Budgetr.Shared.Services;

/// <summary>
/// Interface for time tracking operations.
/// </summary>
public interface ITimeTrackingService
{
    /// <summary>
    /// Gets the current time account state.
    /// </summary>
    TimeAccount Account { get; }
    
    /// <summary>
    /// Gets the current balance, calculated in real-time.
    /// </summary>
    TimeSpan GetCurrentBalance();
    
    /// <summary>
    /// Gets the currently active meter event, if any.
    /// </summary>
    MeterEvent? GetActiveEvent();
    
    /// <summary>
    /// Activates a meter by its ID. Deactivates any currently active meter first.
    /// </summary>
    void ActivateMeter(Guid meterId);
    
    /// <summary>
    /// Deactivates the currently active meter.
    /// </summary>
    void DeactivateMeter();
    
    /// <summary>
    /// Gets timeline data points for the specified period.
    /// </summary>
    List<TimelineDataPoint> GetTimelineData(TimeSpan period);
    
    /// <summary>
    /// Saves the current state to persistent storage.
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Loads the state from persistent storage.
    /// </summary>
    Task LoadAsync();
    
    /// <summary>
    /// Event raised when the account state changes.
    /// </summary>
    event Action? OnStateChanged;
}

/// <summary>
/// Represents a point on the timeline graph.
/// </summary>
public class TimelineDataPoint
{
    public DateTime Timestamp { get; set; }
    public double BalanceHours { get; set; }
}
