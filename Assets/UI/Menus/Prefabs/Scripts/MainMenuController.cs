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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    void Awake()
    {
        // No DontDestroyOnLoad, as we do not need/want MainMenu to persist into GameScene.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Main Menu Panel(s)")]
    public GameObject mainMenuPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button exitButton;

    void Start()
    {
        // Wiring up main menu buttons.
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGameButton);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettingsButton);
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGameButton);
        }
    }

    // Starts the game - tutorial will auto-start if needed.

    public void PlayGameButton()
    {
        Debug.Log("[MainMenuController] Play button clicked - loading game scene.");

        // Loading game scene through MenuManager.
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.EnteringGame();
        }
    }

    public void OpenSettingsButton()
    {
        Debug.Log("[MainMenuController] Settings button clicked");

        // Controller calling PauseMenuManager for business logic.
        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.OpenSettings();
    }

    public void ExitGameButton()
    {
        Debug.Log("[MainMenuController] Attempting to exit game.");
        if (MenuManager.Instance != null)
            MenuManager.Instance.ExitGame();
    }

    // Hides the mainMenu.
    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    // Shows the mainMenu.
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}
