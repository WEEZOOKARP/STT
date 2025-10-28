using UnityEngine;
using UnityEngine.UI;

// Tutorial UI - Displays in-game hints that pause gameplay.
// Allows player to dismiss hints by tapping.
// Integrates with GamePauseManager to block menu access while hints are active.
// Created by Archie Armstrong | 28/10/2025

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    [Header("Hint Display")]
    public GameObject hintPanel;
    public Text hintText;
    public Button hintDismissButton;

    void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to tutorial events.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.OnStepStarted += DisplayTutorialHint;
            TutorialManager.OnTutorialCompleted += OnTutorialComplete;
        }

        // Wiring up dismiss button.
        if (hintDismissButton != null)
        {
            hintDismissButton.onClick.AddListener(DismissTutorialHint);
        }

        // Ensuring panel is initially hidden.
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    // Display tutorial hint and pause game.
    public void DisplayTutorialHint(TutorialStep step)
    {
        if (step == null)
            return;

        // Setting hint text from tutorial step.
        if (hintText != null)
        {
            hintText.text = step.instructionText;
        }

        // Activating hint panel to show.
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
        }

        // Setting pause reason to tutorial hint.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.TutorialHint);
        }

        // Pausing game time.
        Time.timeScale = 0f;

        Debug.Log("[TutorialUI] Hint displayed and game paused");
    }

    // Dismiss hint and resume game.
    public void DismissTutorialHint()
    {
        // Hiding hint panel.
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }

        // Resuming game time.
        Time.timeScale = 1f;

        // Clearing pause reason.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.NotPaused);
        }

        // Advancing tutorial to next step.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.AdvanceToNextStep();
        }

        Debug.Log("[TutorialUI] Hint dismissed and game resumed");
    }

    // Called when tutorial completes.
    public void OnTutorialComplete()
    {
        Debug.Log("[TutorialUI] Tutorial completed");

        // Hide panel if tutorial is done.
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from tutorial events to prevent memory leaks.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.OnStepStarted -= DisplayTutorialHint;
            TutorialManager.OnTutorialCompleted -= OnTutorialComplete;
        }
    }
}
