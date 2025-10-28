/*
 * TutorialManager.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 14/09/2025
 *
 * Last Updated on: ? | BY:
 * What:
 * Why:
 *
 * Purpose: Central controller for the tutorial system.
 * Manages the tutorial flow, step progression, and AR-Specific behaviour.
 *
 * Dependencies: TutorialStep, TutorialCondition
 *
 * Integration Points:
 * - Called by GameManager to check tutorial status.
 * - Extended by specific condition types (e.g. Time-based, Object interaction, etc).
 * - Used by TutorialStep for completion logic.
 * - Managed by TutorialManager for state tracking.
 */

using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // Singleton instance so that other systems may call TutorialManager.Instance from anywhere.
    public static TutorialManager Instance { get; private set; }
    public static event Action<TutorialStep> OnStepStarted;
    public static event Action OnTutorialCompleted;
    public static event System.Action<HintTrigger> OnHintTriggered;

    // TutorialManager.Instance.StartTutorial();

    [Header("Current Step Tracking")] // Label in inspector.
    public int currentStepIndex = 0; // Index of active step.
    public TutorialStep currentStep; // Cached reference to active step.
    public TutorialCondition currentCondition; // Cached reference to active step's conditon.

    void Awake()
    {
        // Keeping the first TutorialManager instance and making sure it persists across all scenes.
        // Additionally, destroying any extras that may appear (e.g., from scene reloads or duplicated prefabs).
        if (Instance == null)
        {
            Instance = this; // The global handle.
            DontDestroyOnLoad(gameObject); // Ensuring this Manager is persistent across scenes.
        }
        else
        {
            Destroy(gameObject); // Preventing duplicates from coexisting.
        }
    }

    [Header("Tutorial State")] // Label in inspector.
    public TutorialState currentState = TutorialState.Idle; // Explicit mode for predicatbility.

    public enum TutorialState
    {
        Idle, // Not running tutorial.
        Running, // Tutorial is active - steps being shown.
        Complete, // Tutorial has been finished.
    }

    [Header("Tutorial Configuration")] // Label in inspector.
    public TutorialStep[] tutorialSteps; // Fixed, ordered steps designers may assign in the inspector.

    // Normal entry point: starts tutorial only if not already completed.
    public void StartTutorial()
    {
        // Checking if tutorial has been completed.
        if (HasCompletedTutorial())
        {
            Debug.Log("Tutorial has already been completed.");
            return;
        }
        Debug.Log("Starting tutorial...");
        StartTutorialInternal();
    }

    void Update()
    {
        // Only do per-frame work while the tutorial is running.
        if (currentState != TutorialState.Running)
            return;

        // TODO: Add call to small method to check completion each frame.
        CheckCurrentStepCompletion();
    }

    // Replay entry point: always starts, even if completed (for settings button or QA).
    public void StartTutorialForced()
    {
        StartTutorialInternal();
    }

    // Clewared stored completion so a normal StartTutorial will run again next time.
    public void ResetTutorialProgress()
    {
        if (MetaProgression.Instance != null)
        {
            // Clearing completion flag.
            MetaProgression.Instance.GetData().hasCompletedTutorial = false;

            // Persisting the change.
            MetaProgression.Instance.SaveData();

            Debug.Log("Tutorial progress succesffuly reset and saved to MetaProgression.");
        }
        else
        {
            Debug.LogError("Cannot reset tutorial progression - MetaProgression not avaliable.");
        }
    }

    // Added debug methods to TutorialManager.cs - Archie | [26/09/25].
    [ContextMenu("Force Start Tutorial")]
    public void ForceStartTutorialDebug()
    {
        Debug.Log("=== FORCE STARTING TUTORIAL ===");
        StartTutorialForced();
    }

    [ContextMenu("Check Tutorial Status")]
    public void CheckTutorialStatusDebug()
    {
        Debug.Log($"HasCompletedTutorial: {HasCompletedTutorial()}");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Tutorial Steps Count: {tutorialSteps?.Length ?? 0}");
    }

    // Returns true if the tutoril was completed in a prior session.
    public bool HasCompletedTutorial()
    {
        // Checking if MetaProgression has been initialized at start up.
        if (MetaProgression.Instance == null)
        {
            Debug.LogWarning("MetaProgression not initalized at start up. Returning false.");
            return false;
        }
        // No list search or string comparison, boolean check.
        return MetaProgression.Instance.GetData().hasCompletedTutorial;
    }

    // Marks the tutorial done and persists this fact.
    void CompleteTutorial()
    {
        currentState = TutorialState.Complete;

        // Checking if MetaProgression is null - Defensive.
        if (MetaProgression.Instance != null)
        {
            // Direct assignment.
            MetaProgression.Instance.GetData().hasCompletedTutorial = true;

            // Saving change to persist.
            MetaProgression.Instance.SaveData();

            Debug.Log("Tutorial completed and saved to MetaProgression.");
        }
        else
        {
            Debug.LogError("Cannot save the tutorial completion - MetaProgression not available.");
        }
        // Invvoking to notify other systems - UI, GameManager, etc.
        OnTutorialCompleted?.Invoke();
    }

    // Shared set up used by both StartTutorial and StartTutorialForced.
    void StartTutorialInternal()
    {
        // Defensive: verifies designer assigned steps in inspector.
        if (tutorialSteps == null || tutorialSteps.Length == 0)
        {
            Debug.LogWarning("No tutorial steps configured!");
            return;
        }

        // Reseting and running the tutorial from first step.
        currentState = TutorialState.Running; // From now on, updates will poll completion.
        currentStepIndex = 0;
        BeginStep(currentStepIndex);
    }

    // Starts a specific step: caches references and lets the condiditon start listening (events, inputs, etc...).
    void BeginStep(int index)
    {
        if (index < 0 || index >= tutorialSteps.Length)
        {
            Debug.LogWarning($"Invalid step index: {index}");
            CompleteTutorial(); // Fail safe - end cleanly if misconfiguration found.
            return;
        }

        currentStep = tutorialSteps[index];
        currentCondition = currentStep != null ? currentStep.completionCondition : null;

        // Diagnostic logging.
        Debug.Log(
            $"[TutorialManager] Event has {OnStepStarted?.GetInvocationList()?.Length ?? 0} subscribers"
        );

        // Each condition decides how to listen (subscribe to events, poll inputs, timers, etc).
        if (currentCondition != null)
        {
            currentCondition.ResetCondition(); // Default - clears any prior completion.
            currentCondition.StartCondition(); // Begins listening for this steps requirement(s).
        }

        Debug.Log($"[TutorialManager] Firing OnStepStarted event for step {index}");
        OnStepStarted?.Invoke(currentStep);

        Debug.Log($"Tutorial step {index} started: {currentStep?.stepName}");
    }

    // Polls the active condition once per frame to see if its done or not.
    void CheckCurrentStepCompletion()
    {
        if (currentCondition == null)
            return;

        if (currentCondition.IsConditionMet())
        {
            CompleteCurrentStep();
        }
    }

    // Cleanly finishes the step - stops listening, then advances or finishes tutorial.
    void CompleteCurrentStep()
    {
        if (currentCondition != null)
            currentCondition.StopCondition();

        Debug.Log($"Tutorial step {currentStepIndex} completed: {currentStep?.stepName}");

        currentStepIndex++;
        if (currentStepIndex < tutorialSteps.Length)
            BeginStep(currentStepIndex);
        else
            CompleteTutorial();
    }

    public void AdvanceToNextStep()
    {
        TutorialManager.Instance.CompleteCurrentStep();
    }

    [Header("In-Game Hints")]
    public HintTrigger[] inGameHints; // Hints that trigger during gameplay

    void Start()
    {
        // Subscribing to gameplay events for in-game hints.
        if (GunController.Instance != null)
        {
            GunController.OnShotFired += CheckHintTriggers;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart += OnWaveStart;
            WaveManager.Instance.OnWaveComplete += OnWaveComplete;
        }

        // Subscribing to enemy spawns.
        EnemyBehavior.OnEnemySpawned += OnEnemySpawned;
    }

    void OnDestroy()
    {
        // Unsubscribe from enemy spawns to avoid memory leaks
        EnemyBehavior.OnEnemySpawned -= OnEnemySpawned;
    }

    void OnWaveStart(int waveNumber)
    {
        CheckHintTriggers("WaveStart", waveNumber);
    }

    void OnWaveComplete(int waveNumber)
    {
        CheckHintTriggers("WaveComplete", waveNumber);
    }

    public void CheckHintTriggers(string triggerType = "")
    {
        if (!SettingsManager.Instance?.currentSettings.tutorialEnabled ?? true)
            return;

        foreach (HintTrigger hint in inGameHints)
        {
            if (ShouldTriggerHint(hint))
            {
                ShowHint(hint);
            }
        }
    }

    // Called when an enemy spawns.
    void OnEnemySpawned(string enemyTypeName, bool isBoss)
    {
        Debug.Log($"[TutorialManager] Enemy spawned: {enemyTypeName} (boss: {isBoss})");

        if (isBoss)
        {
            // Checking for boss hint.
            foreach (HintTrigger hint in inGameHints)
            {
                if (
                    hint.triggerType == HintTriggerType.BossEncountered
                    && TutorialHintTracker.Instance?.ShouldShowHint(hint.hintId) == true
                )
                {
                    ShowHint(hint);
                    return;
                }
            }
        }
        else
        {
            // Checking for specific enemy type hint.
            foreach (HintTrigger hint in inGameHints)
            {
                if (
                    hint.triggerType == HintTriggerType.EnemyTypeEncountered
                    && hint.enemyTypeName == enemyTypeName
                    && TutorialHintTracker.Instance?.ShouldShowHint(hint.hintId) == true
                )
                {
                    ShowHint(hint);
                    return;
                }
            }
        }
    }

    bool ShouldTriggerHint(HintTrigger hint)
    {
        // Don't show if tutorials disabled.
        if (!SettingsManager.Instance?.currentSettings.tutorialEnabled ?? true)
            return false;

        // Making sure not to show if already shown.
        if (!TutorialHintTracker.Instance?.ShouldShowHint(hint.hintId) ?? true)
            return false;

        // Checking the trigger conditions.
        return hint.triggerType switch
        {
            HintTriggerType.GameStart => true, // Always trigger at game start.
            HintTriggerType.EnemyTypeEncountered => true, // Will be checked by event handler.
            HintTriggerType.BossEncountered => true, // Will be checked by event handler.
            HintTriggerType.WaveStart => true, // Wave manager calls this.
            HintTriggerType.BuildPhaseStart => true, // Build phase calls this.
            _ => false,
        };
    }

    public void ShowHint(HintTrigger hint)
    {
        // Mark as shown
        TutorialHintTracker.Instance?.MarkHintShown(hint.hintId);

        // Fire event for TutorialUI to listen to
        OnStepStarted?.Invoke(hint.associatedStep);
        OnHintTriggered?.Invoke(hint);

        Debug.Log($"[TutorialManager] Showing hint: {hint.hintId}");
    }

    // Singleton pattern for global access.
    // Tutorial State Management.
    // Step progression Logic.
    // AR-specific handling.
}
