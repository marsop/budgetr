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
    /// Gets or sets the current timeline period.
    /// </summary>
    TimeSpan TimelinePeriod { get; set; }
    
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
    /// Deletes an event by its ID. Updates balance and triggers state change.
    /// </summary>
    void DeleteEvent(Guid eventId);
    
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
    
    /// <summary>
    /// Exports all data (meters and events) as a JSON string.
    /// </summary>
    string ExportData();
    
    /// <summary>
    /// Imports data from a JSON string, replacing current meters and events.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when import data has duplicate meter factors.</exception>
    Task ImportDataAsync(string json);
    
    /// <summary>
    /// Renames a meter. Only affects future events; existing events keep the old name.
    /// </summary>
    /// <param name="meterId">The ID of the meter to rename.</param>
    /// <param name="newName">The new name (1-40 characters).</param>
    /// <exception cref="ArgumentException">Thrown when the name is invalid.</exception>
    void RenameMeter(Guid meterId, string newName);

    /// <summary>
    /// Deletes a meter by its ID.
    /// </summary>
    /// <param name="meterId">The ID of the meter to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to delete an active meter.</exception>
    void DeleteMeter(Guid meterId);

    /// <summary>
    /// Adds a new meter with the specified name and factor.
    /// </summary>
    /// <param name="name">Name of the meter.</param>
    /// <param name="factor">Factor for the meter.</param>
    /// <exception cref="ArgumentException">Thrown when name is invalid or factor is duplicate/invalid.</exception>
    void AddMeter(string name, double factor);
}

/// <summary>
/// Represents a point on the timeline graph.
/// </summary>
public class TimelineDataPoint
{
    public DateTimeOffset Timestamp { get; set; }
    public double BalanceHours { get; set; }
}
