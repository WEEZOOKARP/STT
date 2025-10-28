/*
 * MenuManager.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 26/09/2025
 *
 * Purpose: Central controller for all menu systems.
 * Manages menu state, transitions, and provides global menu access.
 *
 * Dependencies: None (Singleton)
 *
 * Integration Points:
 * - Called by InputManager for ESC key handling.
 * - Used by Tutorial system for menu-based tutorials.
 * - Manages scene transitions between Game and MainMenu.
 */

using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Singleton instance for global access.
    public static MenuManager Instance { get; private set; }

    // Events for tutorial system integration.
    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;

    [Header("Menu State")]
    public MenuStates currentMenuState = MenuStates.MainMenu;

    [Header("Current Scene")]
    public SceneNames currentScene = SceneNames.MainMenu;

    void Awake()
    {
        // Singleton pattern.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() { }

    void Update() { }

    // Used to find the parent menu of settings using the current scene.
    public void FindSettingsParentMenu()
    {
        if (SettingsMenuManager.Instance != null)
        {
            if (SceneMenuManager.GetScene() == SceneNames.MainMenu)
            {
                SettingsMenuManager.SetParentMenu(SettingsParentMenuType.MainMenu);
            }
            else
            {
                SettingsMenuManager.SetParentMenu(SettingsParentMenuType.PauseMenu);
            }
        }
    }

    // Closes the pause menu and resumes the game.
    public void ResumeGame()
    {
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.ClosePauseMenu();
        }
    }

    // Returns to main menu scene.
    public void ReturnToMainMenu()
    {
        Debug.Log("[MenuManager] Returning to main menu");

        // Resuming time before scene change.
        Time.timeScale = 1f; // Does TimeScale matter? What does it do?

        // Loading main menu scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MainMenu);
    }

    public void EnteringGame()
    {
        Debug.Log("[MenuManager] Entering Game.");

        // Resuming time before scene change.
        Time.timeScale = 1f; // Does TimeScale matter? What does it do?

        // Loading game scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Game);
    }

    // Method called when exiting settings menu to parent menu.
    public void ReturnToParentMenu(SettingsParentMenuType settingsParent)
    {
        if (settingsParent.name == SettingsParentMenuType.MainMenu)
        {
            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.Instance.ShowMainMenu();
                Debug.Log("[MenuManager] Attempting to open settings parent menu - Main Menu.");
            }
        }
        else if (settingsParent.name == SettingsParentMenuType.PauseMenu)
        {
            if (PauseMenuManager.Instance != null)
            {
                PauseMenuManager.OpenPauseMenu();
                Debug.Log("[MenuManager] Attempting to open settings parent menu - Pause Menu.");
            }
        }
    }

    // Method to hide parent menu while settings opens/is open.
    public void HideParentMenu(SettingsParentMenuType settingsParent)
    {
        if (settingsParent.name == SettingsParentMenuType.MainMenu)
        {
            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.CloseMainMenu(); // Implement in MainMenuManager.
                Debug.Log("[MenuManager] Attempting to close main menu to open settings.");
            }
        }
        if (settingsParent.name == SettingsParentMenuType.PauseMenu)
        {
            if (PauseMenuManager.Instance != null && PauseMenuController.Instance != null)
            {
                // Hiding and caching pauseMenu only, thus not unfreezing game.
                PauseMenuController.Instance.HidePauseMenuDisplay();
                PauseMenuManager.Instance.CachePauseMenu();
                Debug.Log("[MenuManager] Attempting to close pause menu to open settings.");
            }
        }
    }

    // Exits the application.
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
