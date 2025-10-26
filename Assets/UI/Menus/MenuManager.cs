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

    [Header("Settings Menu's - Parent Menu")]
    public string settingsParentMenu = ""; // "MainMenu" or "PauseMenu".

    // Events for tutorial system integration.
    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;

    [Header("Menu State")]
    public bool isMenuOpen = false;
    public MenuState currentMenuState = MenuState.Closed;

    [Header("Current Scene Type")]
    public SceneType currentSceneType = SceneType.MainMenu;

    [Header("Menu References")]
    public GameObject pauseMenuCanvas = null;
    public GameObject settingsMenuCanvas = null;
    public GameObject tutorialOverlayCanvas = null;

    public enum MenuState
    {
        Closed, // TODO: Do we need closed here? Do we not just figure this in persistentManager?
        PauseMenu,
        MainMenu,
        Settings,
    }

    public enum SceneType
    {
        GameScene,
        MainMenuScene,
    }

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

    void Start()
    {
        // Initialize menu state.
        CloseAllMenus();
    }

    void Update()
    {
        // Handle ESC key for menu toggle.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // During tutorial, only open menu (don't toggle) - Added by Archie [26/09/25]
            // Purpose: Allow tutorial to complete when menu opens, but keep menu open for user interaction.
            if (
                TutorialManager.Instance != null
                && TutorialManager.Instance.currentState == TutorialManager.TutorialState.Running
            )
            {
                if (currentMenuState == MenuState.Closed)
                {
                    OpenPauseMenu();
                }
                else if (currentMenuState == MenuState.PauseMenu)
                {
                    // During tutorial, ESC can close the menu too.
                    CloseMenu();
                }
            }
            else
            {
                // Normal gameplay - toggle menu.
                TogglePauseMenu();
            }
        }

        // TEST: Press T to close menu - Added by Archie [26/09/25]
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key pressed - calling CloseMenu()");
            CloseMenu();
        }
    }

    // Support for opening settings.
    public void OpenSettingsMenu(string parentMenu = "")
    {
        // Checking that a settings menu instance exists and if not, initializing one.
        if (!PersistentMenuManager.Instance.doesGameMenuExist("SettingsMenu"))
        {
            // Creating settingsMenu and providing references of said menu to MenuManager.cs
            PersistentMenuManager.Instance.SmartMenuCreation("settingsMenuOnly");
            PersistentMenuManager.Instance.ProvidingMenuReferencesToMenuManager();
        }

        // SceneType determines which controller we are to call.
        if (currentSceneType == SceneType.MainMenuScene)
        {
            // For MainMenu - Calling MainMenuController to handle display.
            if (MainMenuController.Instance != null)
            {
                MainMenuController.Instance.ShowSettingPanel();
            }
        }
        else if (currentSceneType == SceneType.GameScene)
        {
            // For PauseMenu - Calling PauseMenuController to handle display.
            if (PauseMenuController.Instance != null)
            {
                PauseMenuController.Instance.ShowSettingPanel();
            }
        }

        // Business Logic.
        settingsParentMenu = parentMenu;
        currentMenuState = MenuState.Settings;
    }

    // Support for closing settings.
    // This is actually broken.
    // TODO: Fix this AHAAHSHAHHSHAGSaUsbaou!
    public void CloseSettingsMenu()
    {
        if (settingsMenuCanvas != null)
        {
            if (SettingsMenuController.Instance != null)
            {
                SettingsMenuController.Instance.closeSettingsDisplay();
            }
        }
        // Caching the SettingsMenu.
        PersistentMenuManager.Instance.CloseMenu("SettingsMenu");

        // Return to parent menu based on stored context.
        if (settingsParentMenu == "MainMenu")
        {
            // Handle MainMenu display logic.
            currentMenuState = MenuState.MainMenu;
            // Asking MainMenuManager to show mainMenu.
            if (MainMenuController.Instance != null)
            {
                MainMenuController.Instance.ShowMainMenu();
            }
        }
        else if (settingsParentMenu == "PauseMenu")
        {
            currentMenuState = MenuState.PauseMenu;
            // Handle PauseMenu display logic.
            if (PersistentMenuManager.Instance.doesGameMenuExist("PauseMenu"))
            {
                PersistentMenuManager.Instance.OpenMenu("PauseMenu");
            }
        }
    }

    bool doesMenuExist(string menuType)
    {
        if (menuType == "PauseMenu")
        {
            if (PersistentMenuManager.Instance.currentPauseMenuInstance != null)
            {
                return true;
            }
        }
        else if (menuType == "MainMenu")
        {
            if (MainMenuManager.Instance.currentMainMenuInstance != null)
            {
                return true;
            }
        }
        // Menu does not exist.
        return false;
    }

    // Opens the pause menu.
    public void OpenPauseMenu()
    {
        if (!PersistentMenuManager.Instance.doesGameMenuExist("PauseMenu"))
        {
            PersistentMenuManager.Instance.SmartMenuCreation("pauseMenuOnly");
            PersistentMenuManager.Instance.ProvidingMenuReferencesToMenuManager();
        }

        // MenuManager to handle display of PauseMenu, such as it does for SettingsMenu.
        currentMenuState = MenuState.PauseMenu;
        if (pauseMenuCanvas != null)
        {
            // REDUNDANT - Display logic for settings; PauseMenuController.cs
            pauseMenuCanvas.SetActive(true);
        }

        // Updating PersistentMenuManager state.
        PersistentMenuManager.Instance.OpenMenu("PauseMenu");
    }

    // Closes all menus and resumes the game.
    public void ClosePauseMenu()
    {
        if (currentMenuState != MenuState.PauseMenu)
            return;

        Debug.Log("[MenuManager] Closing pause menu");

        CloseAllMenus();

        // Always resume game when closing menu - Added by Archie [26/09/25]
        Time.timeScale = 1f;

        // Notify tutorial system
        OnMenuClosed?.Invoke();
    }

    // Simple method for UI buttons - closes any open menu.
    // Added by Archie [26/09/25] - Purpose: Simplified method for button OnClick events.
    public void CloseMenu()
    {
        Debug.Log($"[MenuManager] CloseMenu() called - currentMenuState: {currentMenuState}");
        if (currentMenuState == MenuState.PauseMenu)
        {
            Debug.Log("[MenuManager] Calling ClosePauseMenu()");
            ClosePauseMenu();
        }
        else
        {
            Debug.Log(
                $"[MenuManager] Not in PauseMenu state, cannot close. Current state: {currentMenuState}"
            );
        }
    }

    // Test method to verify button connections work - Added by Archie [26/09/25]
    public void TestButtonConnection()
    {
        Debug.Log("*** BUTTON CONNECTION TEST - THIS SHOULD APPEAR WHEN BUTTON IS CLICKED ***");
        Debug.Log($"MenuManager Instance: {Instance}");
        Debug.Log($"pauseMenuCanvas: {pauseMenuCanvas}");
        Debug.Log($"currentMenuState: {currentMenuState}");
        Debug.Log($"Time.timeScale: {Time.timeScale}");

        // Test if this fixes the issue
        Time.timeScale = 1f;
        Debug.Log("Set Time.timeScale back to 1f for testing");
    }

    // Toggles the pause menu on/off.
    public void TogglePauseMenu()
    {
        if (currentMenuState == MenuState.Closed)
        {
            OpenPauseMenu();
        }
        else if (currentMenuState == MenuState.PauseMenu)
        {
            ClosePauseMenu();
        }
    }

    // Shows tutorial overlays for menu explanation.
    // REDUNDANT CURRENTLY - Tutorial hints and otherwise overlays need to be
    // managed elsewhere.
    public void ShowTutorialOverlay()
    {
        Debug.Log("[MenuManager] Showing tutorial overlay");

        // currentMenuState = MenuState.TutorialOverlay;

        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(true);
    }

    // Hides tutorial overlays.
    public void HideTutorialOverlay()
    {
        Debug.Log("[MenuManager] Hiding tutorial overlay");

        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(false);

        currentMenuState = MenuState.PauseMenu;
    }

    // Closes all menu canvases.
    void CloseAllMenus()
    {
        currentMenuState = MenuState.Closed;
        isMenuOpen = false;

        if (pauseMenuCanvas != null)
            // REDUNDANT - Display logic for settings; PauseMenuController.cs
            pauseMenuCanvas.SetActive(false);
        if (settingsMenuCanvas != null)
            // REDUNDANT - Display logic for settings; SettingsMenuController.cs
            settingsMenuCanvas.SetActive(false);
        // TODO: Figure out how to move tutorial management explicitly to a separate part
        // of project as it should be (and some logic already is).
        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(false);
    }

    // Returns to main menu scene.
    public void ReturnToMainMenu()
    {
        Debug.Log("[MenuManager] Returning to main menu");

        // Resume time before scene change
        Time.timeScale = 1f;

        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Quits the application.
    public void QuitGame()
    {
        Debug.Log("[MenuManager] Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
