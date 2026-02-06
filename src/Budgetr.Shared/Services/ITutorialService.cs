using System;
using System.Threading.Tasks;

namespace Budgetr.Shared.Services;

public record TutorialStep(string Message, string? Route = null, string? ImageUrl = null);

public interface ITutorialService
{
    bool IsActive { get; }
    TutorialStep? CurrentStep { get; }
    
    event Action? OnChange;

    Task InitializeAsync();
    void StartTutorial();
    void NextStep();
    Task CompleteTutorialAsync();
    Task ResetTutorialAsync();
}
