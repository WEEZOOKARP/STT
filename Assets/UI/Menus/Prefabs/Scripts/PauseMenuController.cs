/*
 * PauseMenuController.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Controls the pause menu UI and button interactions.
 * Handles resume, settings, main menu, and quit functionality.
 *
 * Dependencies: MenuManager
 *
 * Integration Points:
 * - Used by MenuManager for pause menu display
 * - Integrates with tutorial system for menu tutorials
 */

using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    // PauseMenu does not need to persist into MainMenu scene, removed DontDestroyOnLoad.
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button tutorialButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Pause Menu Panel")]
    public GameObject mainPausePanel;

    void Start()
    {
        //  Button events.
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettingsButton);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    // Resumes the game and closes the pause menu.
    public void ResumeGame()
    {
        Debug.Log("[PauseMenuController] Resume button clicked");

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ResumeGame();
        }
    }

    public void OpenSettingsButton()
    {
        Debug.Log("[PauseMenuController] Settings button clicked");

        // Controller calling PauseMenuManager for business logic.
        if (PauseMenuManager.Instance != null)
            PauseMenuManager.Instance.OpenSettings();
    }

    // DISPLAY ONLY: Shows the pause menu panel.
    // NOTE: Lifecycle (state management) is handled by PauseMenuManager.
    // Called by: MenuManager (after PauseMenuManager updates state).
    public void ShowPauseMenuDisplay()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
            Debug.Log("[PauseMenuController] Pause menu displayed");
        }
    }

    // DISPLAY ONLY: Hides the pause menu panel.
    // NOTE: Lifecycle (state management) is handled by PauseMenuManager.
    // Called by: MenuManager (after PauseMenuManager updates state).
    public void HidePauseMenuDisplay()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(false);
            Debug.Log("[PauseMenuController] Pause menu hidden");
        }
    }

    // Returning to mainMenu.
    public void ReturnToMainMenu()
    {
        Debug.Log("[PauseMenuController] Main Menu button clicked");

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ReturnToMainMenu();
        }
    }
}
