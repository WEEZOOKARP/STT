/*
 * TutorialUI.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Displays in-game tutorial hints that pause gameplay.
 * Player taps the hint panel to dismiss and resume gameplay.
 * Blocks menu access while hint is active.
 *
 * Dependencies: TutorialManager, GamePauseManager
 *
 * Integration Points:
 * - Subscribed to TutorialManager.OnStepStarted for hint display.
 * - Calls GamePauseManager to pause/resume game.
 * - Blocks PauseButton input while active.
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialHintPanel;
    public TextMeshProUGUI hintText;
    public Button hintDismissButton;
    public Image hintBackground;

    [Header("Hint Settings")]
    public Color hintBackgroundColor = new Color(0, 0, 0, 0.7f);

    void Start()
    {
        // Subscribe to tutorial events.
        TutorialManager.OnStepStarted += OnTutorialStepStarted;
        TutorialManager.OnTutorialCompleted += OnTutorialCompleted;

        Button dismissButton = tutorialPanel.GetComponent<Button>();
        // Wire up dismiss button
        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(DismissTutorialHint);
        }

        // Hiding hint panel initially.
        HideTutorialHint();
    }

    void OnDestroy()
    {
        // Unsubscribe from tutorial events.
        TutorialManager.OnStepStarted -= OnTutorialStepStarted;
        TutorialManager.OnTutorialCompleted -= OnTutorialCompleted;
    }

    // Called when tutorial manager advances to new step.
    void OnTutorialStepStarted(TutorialStep step)
    {
        if (step == null)
        {
            Debug.LogWarning("[TutorialUI] Tutorial step is null");
            return;
        }

        // Display the hint
        DisplayTutorialHint(step.instructionText);

        if (GamePauseManager.Instance != null) 
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.TutorialHint);
            Time.timeScale = 0f;
        }
    }

    // Displaying a tutorial hint and pausing the game.
    void DisplayTutorialHint(string hintMessage)
    {
        Debug.Log($"[TutorialUI] Displaying hint: {hintMessage}");

        // Setting hint text.
        if (hintText != null)
        {
            hintText.text = hintMessage;
        }

        // Showing hint panel.
        if (tutorialHintPanel != null)
        {
            tutorialHintPanel.SetActive(true);
        }

        // Pausing the game for the hint.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.TutorialHint);
            Time.timeScale = 0f;
        }
    }

    // Called when player clicks/taps the hint panel to dismiss.
    public void DismissTutorialHint()
    {
        Debug.Log("[TutorialUI] Hint dismissed by player");

        // Hiding hint panel.
        HideTutorialHint();

        // Resuming gameplay.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.NotPaused);
            Time.timeScale = 1f;
        }

        // Notify tutorial manager that hint was dismissed.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.AdvanceToNextStep();
        }
    }

    // Hides the tutorial hint panel.
    void HideTutorialHint()
    {
        if (tutorialHintPanel != null)
        {
            tutorialHintPanel.SetActive(false);
        }
    }

    // Called when tutorial system completes.
    void OnTutorialCompleted()
    {
        Debug.Log("[TutorialUI] Tutorial completed");
        HideTutorialHint();
    }

}
