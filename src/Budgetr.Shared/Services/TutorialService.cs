using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Localization;
using Budgetr.Shared.Resources;

namespace Budgetr.Shared.Services;

public class TutorialService : ITutorialService
{
    private readonly IStorageService _storageService;
    private readonly NavigationManager _navigationManager;
    private readonly IStringLocalizer<Strings> _localizer;
    private const string TutorialCompletedKey = "tutorial_completed_v1";
    
    private int _currentStepIndex = -1;
    private List<TutorialStep> _steps = new();

    public bool IsActive => _currentStepIndex >= 0 && _currentStepIndex < _steps.Count;
    public TutorialStep? CurrentStep => IsActive ? _steps[_currentStepIndex] : null;

    public event Action? OnChange;

    public TutorialService(IStorageService storageService, NavigationManager navigationManager, IStringLocalizer<Strings> localizer)
    {
        _storageService = storageService;
        _navigationManager = navigationManager;
        _localizer = localizer;
        
        InitializeSteps();
    }

    private void InitializeSteps()
    {
        _steps = new List<TutorialStep>
        {
            // 1. Intro
            new TutorialStep(_localizer["TutorialIntro"], "", "img/tutorial-avatar-5.png"),
            
            // 2. Overview
            new TutorialStep(_localizer["TutorialOverview"], "", "img/tutorial-avatar-2.png"),
            
            // 3. Meters
            new TutorialStep(_localizer["TutorialMeters"], "meters", "img/tutorial-avatar-3.png"),
            
            // 4. Timeline
            new TutorialStep(_localizer["TutorialTimeline"], "timeline", "img/tutorial-avatar-4.png"),
            
            // 5. History
            new TutorialStep(_localizer["TutorialHistory"], "history", "img/tutorial-avatar.png"),
            
            // 6. Sync
            new TutorialStep(_localizer["TutorialSync"], "sync", "img/tutorial-avatar-2.png"),
            
            // 7. Completion
            new TutorialStep(_localizer["TutorialCompletion"], "", "img/tutorial-avatar-3.png")
        };
    }

    public async Task InitializeAsync()
    {
        var completed = await _storageService.GetItemAsync(TutorialCompletedKey);
        if (completed == null)
        {
            // First time launch!
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        _currentStepIndex = 0;
        NavigateToCurrentStep();
        NotifyStateChanged();
    }

    public void NextStep()
    {
        if (!IsActive) return;

        _currentStepIndex++;

        if (_currentStepIndex >= _steps.Count)
        {
            // Tutorial finished
            _ = CompleteTutorialAsync();
        }
        else
        {
            NavigateToCurrentStep();
            NotifyStateChanged();
        }
    }

    public async Task CompleteTutorialAsync()
    {
        _currentStepIndex = -1;
        NotifyStateChanged();
        await _storageService.SetItemAsync(TutorialCompletedKey, "true");
    }

    public async Task ResetTutorialAsync()
    {
        await _storageService.RemoveItemAsync(TutorialCompletedKey);
        StartTutorial();
    }

    private void NavigateToCurrentStep()
    {
        if (CurrentStep?.Route != null)
        {
            // Check if we are already there to avoid reload if not needed? 
            // NavigationManager handles that gracefully usually.
            _navigationManager.NavigateTo(CurrentStep.Route);
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
