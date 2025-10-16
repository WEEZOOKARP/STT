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
 * - Called by InputManager for ESC key handling
 * - Used by Tutorial system for menu-based tutorials
 * - Manages scene transitions between Game and MainMenu
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
    public bool isMenuOpen = false;
    public MenuState currentMenuState = MenuState.Closed;

    [Header("Menu References")]
    public GameObject pauseMenuCanvas = false;
    public GameObject tutorialOverlayCanvas = false;

    public enum MenuState
    {
        Closed, // TODO: Do we need closed here? Do we not just figure this in persistentManager?
        PauseMenu,
        TutorialOverlay,
        Settings,
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

    // Opens the pause menu and pauses the game.
    public void OpenPauseMenu()
    {
        if (currentMenuState != MenuState.Closed)
            return;

        Debug.Log("[MenuManager] Opening pause menu");

        currentMenuState = MenuState.PauseMenu;
        isMenuOpen = true;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        // Only pause game if not in tutorial - Added by Archie [26/09/25]
        // Purpose: Keep game running during tutorial so UI buttons work.
        if (
            TutorialManager.Instance == null
            || TutorialManager.Instance.currentState != TutorialManager.TutorialState.Running
        )
        {
            Time.timeScale = 0f;
        }

        // Notify tutorial system
        OnMenuOpened?.Invoke();
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
    public void ShowTutorialOverlay()
    {
        Debug.Log("[MenuManager] Showing tutorial overlay");

        currentMenuState = MenuState.TutorialOverlay;

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
            pauseMenuCanvas.SetActive(false);
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
