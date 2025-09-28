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
        if (step == null)
        {
            Debug.LogError("Tutorial step is null.");
            return;
        }
        
        if (instructionText == null)
        {
            Debug.LogError("Instruction text UI component is null.");
            return;
        }
        
        if (string.IsNullOrEmpty(step.instructionText))
        {
            Debug.LogError($"Tutorial step '{step.stepName}' has empty instruction text.");
            return;
        }
        
        instructionText.text = step.instructionText;

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
