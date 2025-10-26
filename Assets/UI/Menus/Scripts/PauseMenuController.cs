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
    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button tutorialButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Pause Menu Panels")]
    public GameObject mainPausePanel;
    public GameObject settingsPanel;

    void Start()
    {
        // Wire up button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (tutorialButton != null)
            tutorialButton.onClick.AddListener(ShowTutorial);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    // Resumes the game and closes the pause menu.
    public void ResumeGame()
    {
        Debug.Log("[PauseMenuController] Resume button clicked");

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ClosePauseMenu();
        }
    }

    // Logic for display.
    public void OpenPauseMenu()
    {
        // Last line of "security" check that we ARE infact in the gameScene.
        if (PersistentMenuManager.Instance.pauseMenuPrefab != null)
        {
            ShowPauseMenu();
        }
        else
        {
            Debug.Log(
                "Cannot open pauseMenu, not currently in gameScene.\n"
                    + "Error thrown from OpenPauseMenu method in PersistentMenuManager.cs!"
            );
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[PauseMenuController] Settings button clicked");

        // Controller calling MenuManager for business logic.
        MenuManager.Instance.OpenSettingsMenu("PauseMenu");
    }

    // --- REDUNDANT ---
    // Helper method for display logic.
    private void ShowSettingPanel()
    {
        // Hide main pause panel, show settings panel.
        if (mainPausePanel != null)
            mainPausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void HidePauseMenu()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(false);
        }
    }

    private void ShowPauseMenu()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
        }
    }

    public void ShowTutorial()
    {
        Debug.Log("[PauseMenuController] Tutorial button clicked");

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowTutorialOverlay();
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

    // Quits the application.
    public void QuitGame()
    {
        Debug.Log("[PauseMenuController] Quit button clicked");

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.QuitGame();
        }
    }

    // Closes settings and returns to main pause menu.
    public void CloseSettings()
    {
        Debug.Log("[PauseMenuController] Closing settings");

        if (SettingsMenuController.Instance != null)
        {
            SettingsMenuController.Instance.CloseSettings();
        }
    }
}
