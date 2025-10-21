using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentMenuManager : MonoBehaviour
{
    public static PersistentMenuManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            // First time - becomes instance.
            Instance = this;
            // Persists scene changes.
            DontDestroyOnLoad(gameObject);
            // TODO: Initialize here.
            SceneDetection();
        }
        else
        {
            // Already have instance - destroying duplicate.
            Destroy(gameObject);
        }
    }

    [Header("In-game Menu Prefab Repository")]
    public GameObject pauseMenuPrefab;
    public GameObject settingsMenuPrefab;

    // public GameObject tutorialOverlayPrefab; - Making separate manager for tutorial.
    // public GameObject tutorialOverlayPrefab; - Making separate manager for tutorial.

    public enum MenuState
    {
        Cached, // Menu is not open/destroyed and is cached.
        Open, // Menu is open and not cached.
        Destroyed, // Menu is not open/cached and is destroyed.
    }

    // MainMenu instance not persisted in here - so no instance tracking needed I believe.
    private GameObject currentSettingsMenuInstance;
    private GameObject currentPauseMenuInstance;

    // Intializing MenuStates.
    private MenuState pauseMenuState = MenuState.Destroyed;
    private MenuState settingsMenuState = MenuState.Destroyed;

    // Scene type enum.

    // -------------------------------------------------------
    // Redundant - currently not utilized.
    // private string currentMenuName;

    // Redundant - am referencing specifics from enum.
    // private MenuState menuState; // Destroyed, Cached, Open.
    // -------------------------------------------------------

    // Smart caching with timer.
    private float pauseMenuCacheTimer = 0f;
    private float settingsMenuCacheTimer = 0f;
    private const float CACHE_DURATION = 30f; // 30 second caching.

    void Update()
    {
        // Handling cache timers for smart caching.
        HandleCacheTimers();
    }

    // Handle cache countdown and cleanup.
    void HandleCacheTimers()
    {
        // Pause menu caching.
        if (pauseMenuState == MenuState.Cached)
        {
            pauseMenuCacheTimer += Time.deltaTime;
            if (pauseMenuCacheTimer >= CACHE_DURATION)
            {
                Debug.Log("[PersistentMenuManager] Cache expired - destroying PauseMenu");
                DestroyPauseMenu();
            }
        }

        // Settings menu caching.
        if (settingsMenuState == MenuState.Cached)
        {
            settingsMenuCacheTimer += Time.deltaTime;
            if (settingsMenuCacheTimer >= CACHE_DURATION)
            {
                Debug.Log("[PersistentMenuManager] Cache expired - destroying SettingsMenu");
                DestroySettingsMenu();
            }
        }
    }

    // Destorying the settingMenu instance.
    public void DestroySettingsMenu()
    {
        if (currentSettingsMenuInstance != null)
        {
            // Destroying and updating settingsMenuState to reflect.
            Destroy(currentSettingsMenuInstance);
            settingsMenuState = MenuState.Destroyed;
            // Reseting the cache timer.
            settingsMenuCacheTimer = 0f;
        }
        else
        {
            Debug.Log("No currentSettingsMenuInstance found to be destroyed.");
        }
    }

    // Destroying the pauseMenu instance.
    public void DestroyPauseMenu()
    {
        if (currentPauseMenuInstance != null)
        {
            // Destroying and updating pauseMenuState to reflect.
            Destroy(currentPauseMenuInstance);
            pauseMenuState = MenuState.Destroyed;
            // Reseting the cache timer.
            pauseMenuCacheTimer = 0f;
        }
        else
        {
            Debug.Log("No currentPauseMenuInstance found to be destoryed.");
        }
    }

    // Scene detection.
    void SceneDetection()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Redundant - currently not required, due to "MainMenu" option within
        // OnSceneLoaded method.
        // SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Handling scene change.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // TODO: Finish implementing scene -> prefab logic here.
        switch (scene.name)
        {
            case "GameScene":
                // Pre-creating menus (hidden) so they're ready when user needs them.
                SmartMenuCreation("pauseMenuOnly");
                // Telling MenuManager where to find them.
                ProvidingMenuReferencesToMenuManager();
                break;
            case "MainMenu":
                // Cleaning up when leaving game.
                DestroyGameMenus();
                MenuManager.Instance.ReturnToMainMenu();
                break;
            default:
                Debug.Log("Error finding current scene: " + scene.name);
                break;
        }
    }

    // Destroys both GameMenus here to prepare to exit to mainMenu.
    void DestroyGameMenus()
    {
        // Destroying settingsMenu first, as it is a 'child' of pauseMenu in a UI sense.
        DestroySettingsMenu();
        Debug.Log("Destroyed settings menu.");

        DestroyPauseMenu();
        Debug.Log("Destroyed pause menu.");
    }

    // Smart menu creation - changed from 'EnsureGameMenusExist()'.
    public void SmartMenuCreation(string menuType)
    {
        // Using inclusion logic to check which menu is being requested.
        if (menuType == "pauseMenuOnly")
        {
            // Only creating menu if it doesn't exist - smart caching.
            if (currentPauseMenuInstance == null)
            {
                currentPauseMenuInstance = Instantiate(pauseMenuPrefab);
                // currentPauseMenuInstance.SetActive(false); // Hidden until needed by user.
                pauseMenuState = MenuState.Cached;
            }
        }

        if (menuType == "settingsMenuOnly")
        {
            // Only creating menu if it doesn't exist - smart caching.
            if (currentSettingsMenuInstance == null)
            {
                currentSettingsMenuInstance = Instantiate(settingsMenuPrefab);
                currentSettingsMenuInstance.SetActive(false); // Hidden until needed by user.
                settingsMenuState = MenuState.Cached;
            }
        }
    }

    // Provifing prefabs to MenuManager.
    public void ProvidingMenuReferencesToMenuManager()
    {
        if (MenuManager.Instance != null)
        {
            // Giving MenuManager the instantiated prefabs.
            // TODO: Fix these implementations.
            MenuManager.Instance.pauseMenuCanvas = currentPauseMenuInstance;
            MenuManager.Instance.settingsMenuCanvas = currentSettingsMenuInstance;
            // MenuManager.Instance.tutorialOverlayCanvas = currentTutorialInstance;
            // - Tutorial manager being made.
        }
    }

    // ===== Logic for display to be moved to PauseMenuController. ======
    // public void OpenPauseMenu()
    // {
    //     // Last line of "security" check that we ARE infact in the gameScene.
    //     if (MenuManager.Instance.currentSceneType == "GameScene")
    //     {
    //         MenuManager.Instance.pauseMenuCanvas.SetActive(true);
    //     } else {
    //         Debug.Log("Cannot open pauseMenu, not currently in gameScene.\n
    //         Error thrown from OpenPauseMenu method in PersistentMenuManager.cs!");
    //     }
    // }

    // Public method to allow other systems to open menu.
    public void OpenMenu(string menuType)
    {
        switch (menuType)
        {
            case "PauseMenu":
                if (currentPauseMenuInstance == null)
                {
                    // Recreating pauseMenu if destroyed.
                    SmartMenuCreation("pauseMenuOnly");
                    // Updating references automatically.
                    ProvidingMenuReferencesToMenuManager();
                    // Using helper method 'OpenPauseMenu' to open PauseMenu.
                    OpenPauseMenu();
                }
                else if (pauseMenuState == MenuState.Cached)
                {
                    // Using helper method 'OpenPauseMenu' to open PauseMenu.
                    OpenPauseMenu();
                    // TODO: Implement opening of ('old') cached menu further.
                    pauseMenuState = MenuState.Open;
                    pauseMenuCacheTimer = 0f;
                }
                break;
            case "SettingsMenu":
                if (currentSettingsMenuInstance == null)
                {
                    // Recreating settingsMenu if destroyed.
                    SmartMenuCreation("settingsMenuOnly");
                    // Updating references automatically.
                    ProvidingMenuReferencesToMenuManager();
                    // TODO: Implement opening of new menu.
                }
                else if (settingsMenuState == MenuState.Cached)
                {
                    // TODO: Implement opening of ('old') cached menu further.
                    settingsMenuState = MenuState.Open;
                    settingsMenuCacheTimer = 0f;
                }
                break;
        }
    }

    // Public method to allow other systems to close menu.
    public void CloseMenu(string menuType)
    {
        switch (menuType)
        {
            // TODO: Figure out how we want the menu's to close and persist hierachy wise.
            // Since to open the settingsMenu (in game), you must first be in the pauseMenu,
            // ideally, the pauseMenu will be cached and persist during the time the settingsMenu
            // is open. Then, upon the settingsMenu's closure, it should still be 'alive' and
            // should appear open.
            // The only times the pauseMenu should close, is when the game is exited by any means
            // (including exiting to the mainMenu), or when the player taps to resume game.
            case "PauseMenu":
                pauseMenuState = MenuState.Cached; // Back to being cached - start timer.
                pauseMenuCacheTimer = 0f; // Reset timer.
                // TODO: Implement the resuming of game.
                break;
            case "SettingsMenu":
                settingsMenuState = MenuState.Cached; // Back to being cached - start timer.
                settingsMenuCacheTimer = 0f; // Reset timer.
                // TODO: Open up pauseMenu (visually).
                break;
        }
    }

    // Method called when checking if menuType exists.
    public bool doesGameMenuExist(string menuType)
    {
        if (menuType == "PauseMenu")
        {
            if (currentPauseMenuInstance != null)
            {
                return true;
            }
        }
        if (menuType == "SettingsMenu")
        {
            if (currentSettingsMenuInstance != null)
            {
                return true;
            }
        }

        return false; // No, it does not.
    }
}
