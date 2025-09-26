using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TextMeshProUGUI instructionText;
    public GameObject tutorialPanel;

    void Start()
    {
        // Immediately checking state.
        // Handling case where the tutorial has already started.
        if (TutorialManager.Instance != null)
        {
            // Checking if tutorial is already running.
            if (TutorialManager.Instance.currentState == TutorialManager.TutorialState.Running)
            {
                // Catching up with the current step immediately.
                OnStepStarted(TutorialManager.Instance.currentStep);
            }
        }

        // Future event subscription - for handling further step changes.
        TutorialManager.OnStepStarted += OnStepStarted;
        TutorialManager.OnTutorialCompleted += OnTutorialCompleted;
    }

    void OnStepStarted(TutorialStep step)
    {
        if (step != null && instructionText != null)
        {
            instructionText.text = step.instructionText;
        }
        else
        {
            Debug.LogError("Tutorial step is null or instruction text is null.");
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    void OnTutorialCompleted()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}
