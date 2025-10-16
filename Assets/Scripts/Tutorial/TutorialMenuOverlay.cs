/*
 * TutorialMenuOverlay.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Displays tutorial overlays explaining menu elements.
 * Shows interactive hints and highlights for menu buttons.
 *
 * Dependencies: MenuManager
 *
 * Integration Points:
 * - Called by MenuManager when tutorial overlay is needed
 * - Used by tutorial system for Step 3 (menu explanation)
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialMenuOverlay : MonoBehaviour
{
    [Header("Tutorial Overlay Elements")]
    public GameObject[] tutorialHints;
    public Button nextHintButton;
    public Button skipTutorialButton;
    public Text currentHintText;
    
    [Header("Highlight Effects")]
    public GameObject resumeHighlight;
    public GameObject settingsHighlight;
    public GameObject mainMenuHighlight;
    
    [Header("Tutorial Content")]
    public string[] hintTexts = new string[]
    {
        "This is the RESUME button - click it to continue playing!",
        "The SETTINGS button lets you adjust game options.",
        "MAIN MENU returns you to the title screen.",
        "Press ESC anytime to open this menu. Tutorial complete!"
    };
    
    private int currentHintIndex = 0;
    
    void Start()
    {
        // Wire up button events
        if (nextHintButton != null)
            nextHintButton.onClick.AddListener(ShowNextHint);
        if (skipTutorialButton != null)
            skipTutorialButton.onClick.AddListener(SkipTutorial);
    }
    
    void OnEnable()
    {
        // Start tutorial overlay sequence
        currentHintIndex = 0;
        StartCoroutine(ShowTutorialSequence());
    }
    
    // Shows the tutorial sequence with highlights and explanations.
    IEnumerator ShowTutorialSequence()
    {
        Debug.Log("[TutorialMenuOverlay] Starting menu tutorial sequence");
        
        // Hide all hints initially
        HideAllHints();
        
        // Show first hint
        ShowCurrentHint();
        
        yield return null;
    }
    
    // Shows the current hint based on currentHintIndex.
    void ShowCurrentHint()
    {
        // Hide all highlights first.
        HideAllHighlights();
        
        // Update hint text.
        if (currentHintText != null && currentHintIndex < hintTexts.Length)
        {
            currentHintText.text = hintTexts[currentHintIndex];
        }
        
        // Show appropriate highlight.
        switch (currentHintIndex)
        {
            case 0: // Resume button.
                if (resumeHighlight != null)
                    resumeHighlight.SetActive(true);
                break;
            case 1: // Settings button.
                if (settingsHighlight != null)
                    settingsHighlight.SetActive(true);
                break;
            case 2: // Main Menu button.
                if (mainMenuHighlight != null)
                    mainMenuHighlight.SetActive(true);
                break;
            case 3: // Final hint.
                // No highlight needed.
                break;
        }
        
        Debug.Log($"[TutorialMenuOverlay] Showing hint {currentHintIndex + 1}/{hintTexts.Length}");
    }
    

    // Advances to the next tutorial hint.
    public void ShowNextHint()
    {
        currentHintIndex++;
        
        if (currentHintIndex >= hintTexts.Length)
        {
            // Tutorial complete
            CompleteTutorial();
        }
        else
        {
            ShowCurrentHint();
        }
    }
    
    // Skips the tutorial and closes overlay.
    public void SkipTutorial()
    {
        Debug.Log("[TutorialMenuOverlay] Tutorial skipped by user");
        CompleteTutorial();
    }
    

    // Completes the tutorial and hides overlay.
    void CompleteTutorial()
    {
        Debug.Log("[TutorialMenuOverlay] Menu tutorial completed");
        
        // Hide all highlights and hints.
        HideAllHighlights();
        HideAllHints();
        
        // Hide tutorial overlay
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.HideTutorialOverlay();
        }
        
        // Notify tutorial system that menu tutorial is complete.
        // This could trigger tutorial step completion.
    }
    

    // Hides all tutorial hints.
    void HideAllHints()
    {
        if (tutorialHints != null)
        {
            foreach (GameObject hint in tutorialHints)
            {
                if (hint != null)
                    hint.SetActive(false);
            }
        }
    }
    
    // Hides all button highlights.
    void HideAllHighlights()
    {
        if (resumeHighlight != null)
            resumeHighlight.SetActive(false);
        if (settingsHighlight != null)
            settingsHighlight.SetActive(false);
        if (mainMenuHighlight != null)
            mainMenuHighlight.SetActive(false);
    }
}
