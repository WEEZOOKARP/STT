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
    // Singleton instance for global access
    public static MenuManager Instance { get; private set; }
    
    // Events for tutorial system integration
    public static event Action OnMenuOpened;
    public static event Action OnMenuClosed;
    
    [Header("Menu State")]
    public bool isMenuOpen = false;
    public MenuState currentMenuState = MenuState.Closed;
    
    [Header("Menu References")]
    public GameObject pauseMenuCanvas;
    public GameObject tutorialOverlayCanvas;
    
    public enum MenuState
    {
        Closed,
        PauseMenu,
        TutorialOverlay,
        Settings
    }
    
    void Awake()
    {
        // Singleton pattern
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
        // Initialize menu state
        CloseAllMenus();
    }
    
    void Update()
    {
        // Handle ESC key for menu toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    
    /// <summary>
    /// Opens the pause menu and pauses the game
    /// </summary>
    public void OpenPauseMenu()
    {
        if (currentMenuState != MenuState.Closed) return;
        
        Debug.Log("[MenuManager] Opening pause menu");
        
        currentMenuState = MenuState.PauseMenu;
        isMenuOpen = true;
        
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);
            
        // Pause game
        Time.timeScale = 0f;
        
        // Notify tutorial system
        OnMenuOpened?.Invoke();
    }
    
    /// <summary>
    /// Closes all menus and resumes the game
    /// </summary>
    public void ClosePauseMenu()
    {
        if (currentMenuState != MenuState.PauseMenu) return;
        
        Debug.Log("[MenuManager] Closing pause menu");
        
        CloseAllMenus();
        
        // Resume game
        Time.timeScale = 1f;
        
        // Notify tutorial system
        OnMenuClosed?.Invoke();
    }
    
    /// <summary>
    /// Toggles the pause menu on/off
    /// </summary>
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
    
    /// <summary>
    /// Shows tutorial overlays for menu explanation
    /// </summary>
    public void ShowTutorialOverlay()
    {
        Debug.Log("[MenuManager] Showing tutorial overlay");
        
        currentMenuState = MenuState.TutorialOverlay;
        
        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(true);
    }
    
    /// <summary>
    /// Hides tutorial overlays
    /// </summary>
    public void HideTutorialOverlay()
    {
        Debug.Log("[MenuManager] Hiding tutorial overlay");
        
        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(false);
            
        currentMenuState = MenuState.PauseMenu;
    }
    
    /// <summary>
    /// Closes all menu canvases
    /// </summary>
    void CloseAllMenus()
    {
        currentMenuState = MenuState.Closed;
        isMenuOpen = false;
        
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        if (tutorialOverlayCanvas != null)
            tutorialOverlayCanvas.SetActive(false);
    }
    
    /// <summary>
    /// Returns to main menu scene
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[MenuManager] Returning to main menu");
        
        // Resume time before scene change
        Time.timeScale = 1f;
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Quits the application
    /// </summary>
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
