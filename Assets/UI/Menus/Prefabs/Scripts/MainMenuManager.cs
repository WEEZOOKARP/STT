using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    private GameObject currentMainMenuInstance;

    [Header("MainMenu Prefab")]
    [SerializeField]
    private GameObject mainMenuPrefab;

    [Header("MainMenu UI References")]
    public GameObject mainMenuCanvas;
    public GameObject settingsMenuCanvas; // Settings opened from MainMenu.

    [Header("MainMenu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    // NOTE: No Caching required for MainMenu.
    // Therefore, no caching logic and no CachingMenuManager references.

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

    public void CreateMainMenu()
    {
        // If MainMenu already exists, no need to create new.
        if (currentMainMenuInstance != null)
        {
            Debug.Log("[MainMenuManager] Main menu already exists - skipping creation.");
            return;
        }

        // Checking prefab is assigned.
        if (mainMenuPrefab == null)
        {
            Debug.LogError("[MainMenuManager] mainMenuPrefab is not assigned!");
            return;
        }

        // Instantiating the menu.
        currentMainMenuInstance = Instantiate(mainMenuPrefab);
        Debug.Log("[MainMenuManager] Main menu instance created");

        // Main Menu cannot be cached.

        // MainMenu must show immediately.
        if (MainMenuController.Instance != null)
        {
            ShowMainMenu();
            Debug.Log("[MainMenuManager] Main menu shown (cannot be cached)");
        }
    }

    // --- MainMenu-specific methods. ---

    // Show mainMenu.
    public void ShowMainMenu()
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.ShowMainMenu();
        }
    }

    // Closes main menu - If settings are opened.
    public void HideMainMenu()
    {
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.HideMainMenu();
        }
    }

    // Usually, this means; The game is started.
    public void CloseMainMenu()
    {
        if (currentMainMenuInstance != null)
        {
            currentMainMenuInstance.SetActive(false);
            Debug.Log("MainMenu should be closed now.");
        }
    }

    public void OpenSettings()
    {
        if (SettingsMenuManager.Instance != null)
            SettingsMenuManager.Instance.OpenSettingsMenu();
    }

    // Destroying the main menu instance.
    public void DestroyMainMenu()
    {
        if (currentMainMenuInstance != null)
        {
            Destroy(currentMainMenuInstance);
            currentMainMenuInstance = null;
        }
        // mainMenuState = MenuStates.Destroyed; - Likely Redundant.
        Debug.Log("[MainMenuManager] Main menu destroyed");
    }
}
