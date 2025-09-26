/*
 * MainMenuController.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Enhanced main menu with tutorial replay functionality.
 * Handles play, tutorial replay, settings, and quit options.
 *
 * Dependencies: TutorialManager, SceneManager.
 *
 * Integration Points:
 * - Integrates with tutorial system for replay functionality.
 * - Manages scene transitions to game scene.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string gameSceneName = "Game";
    
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button tutorialButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Tutorial Integration")]
    public GameObject tutorialReplayPanel;
    public Button confirmTutorialButton;
    public Button cancelTutorialButton;
    
    void Start()
    {
        // Wire up main menu buttons.
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(ShowTutorialOptions);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Wire up tutorial replay buttons
        if (confirmTutorialButton != null)
            confirmTutorialButton.onClick.AddListener(StartTutorialReplay);
        if (cancelTutorialButton != null)
            cancelTutorialButton.onClick.AddListener(HideTutorialOptions);
            
        // Hide tutorial panel initially
        if (tutorialReplayPanel != null)
            tutorialReplayPanel.SetActive(false);
    }
    
    
    // Starts the game - tutorial will auto-start if needed.
    
    public void PlayGame()
    {
        Debug.Log("[MainMenuController] Play button clicked - loading game scene");
        
        // Load game scene - GameManager will handle tutorial check.
        SceneManager.LoadScene(gameSceneName);
    }
    
    
    // Shows tutorial replay options.
    
    public void ShowTutorialOptions()
    {
        Debug.Log("[MainMenuController] Tutorial button clicked");
        
        if (tutorialReplayPanel != null)
        {
            tutorialReplayPanel.SetActive(true);
        }
        else
        {
            // If no panel, directly start tutorial replay.
            StartTutorialReplay();
        }
    }
    
    
    // Hides tutorial replay options.
    
    public void HideTutorialOptions()
    {
        Debug.Log("[MainMenuController] Tutorial options cancelled");
        
        if (tutorialReplayPanel != null)
        {
            tutorialReplayPanel.SetActive(false);
        }
    }
    
    
    // Forces tutorial replay by resetting progress and loading game.
    public void StartTutorialReplay()
    {
        Debug.Log("[MainMenuController] Starting tutorial replay");
        // Reset tutorial progress so it will play again.
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ResetTutorialProgress();
            Debug.Log("[MainMenuController] Tutorial progress reset");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] TutorialManager not found - tutorial may not replay properly");
        }
        
        // Load game scene - tutorial will now start automatically.
        SceneManager.LoadScene(gameSceneName);
    }
    
    
    // Opens the settings menu.
    public void OpenSettings()
    {
        Debug.Log("[MainMenuController] Settings button clicked");
        
        // TODO: Implement settings menu.
        // For now, just show a placeholder message.
        Debug.Log("Settings menu - Coming soon!");
    }
    
    
    // Quits the application.
    public void QuitGame()
    {
        Debug.Log("[MainMenuController] Quit button clicked");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
