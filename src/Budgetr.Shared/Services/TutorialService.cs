using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Budgetr.Shared.Services;

public class TutorialService : ITutorialService
{
    private readonly IStorageService _storageService;
    private readonly NavigationManager _navigationManager;
    private const string TutorialCompletedKey = "tutorial_completed_v1";
    
    private int _currentStepIndex = -1;
    private List<TutorialStep> _steps = new();

    public bool IsActive => _currentStepIndex >= 0 && _currentStepIndex < _steps.Count;
    public TutorialStep? CurrentStep => IsActive ? _steps[_currentStepIndex] : null;

    public event Action? OnChange;

    public TutorialService(IStorageService storageService, NavigationManager navigationManager)
    {
        _storageService = storageService;
        _navigationManager = navigationManager;
        
        InitializeSteps();
    }

    private void InitializeSteps()
    {
        _steps = new List<TutorialStep>
        {
            // 1. Intro
            new TutorialStep("Hi there! Welcome to 'Budgetr'! I'm your personal time budget guide. I'm here to help you get started with managing your time effectively!", "", "img/tutorial-avatar.png"),
            
            // 2. Overview
            new TutorialStep("This is your 'Overview'. It gives you a quick snapshot of your current temporal budget health. You can see your total time balance and active meters at a glance.", "", "img/tutorial-avatar-2.png"),
            
            // 3. Meters
            new TutorialStep("Here in 'Meters', you define your activities! You can set up 'Meters' for things like Work, Study, or Leisure. Think of them as the pulse of your day.", "meters", "img/tutorial-avatar-3.png"),
            
            // 4. Timeline
            new TutorialStep("The 'Timeline' is your visual aid! 🔮 It projects your past time budget based on your recorded activities. Use it to see trends and plan ahead.", "timeline", "img/tutorial-avatar-4.png"),
            
            // 5. History
            new TutorialStep("Want to see what happened in the past? The 'History' page keeps a log of all your entries and changes. It's great for tracking your activity habits.", "history", "img/tutorial-avatar.png"),
            
            // 6. Sync
            new TutorialStep("Finally, 'Sync' keeps your data safe! You can back up your time tracking data to Google Drive or export it locally. Never lose your progress!", "sync", "img/tutorial-avatar-2.png"),
            
            // 7. Completion
            new TutorialStep("That's the basics! You're all set to take control of your time. If you need me again, just look for the help icon. Happy tracking! ✨", "", "img/tutorial-avatar-3.png")
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
