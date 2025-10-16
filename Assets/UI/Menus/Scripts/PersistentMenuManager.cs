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

    [Header("Menu Prefab Repository")]
    public GameObject pauseMenuPrefab;
    public GameObject mainMenuPrefab;
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
    private string currentMenuName;

    private MenuState menuState; // Destroyed, Cached, Open.
    private MenuState pauseMenuState = MenuState.Destroyed;
    private MenuState settingsMenuState = MenuState.Destroyed;

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
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Handling scene change.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // TODO: Finish implementing scene -> prefab logic here.
        switch (scene.name)
        {
            case "GameScene":
                // Pre-creating menus (hidden) so they're ready when user needs them.
                SmartMenuCreation();
                // Telling MenuManager where to find them.
                ProvidingMenuReferencesToMenuManager();
                break;
            case "MainMenu":
                // Cleaning up when leaving game.
                DestroyGameMenus();
                break;
            default:
                Debug.Log("Error finding current scene: " + scene.name);
                break;
        }
    }

    // Smart menu creation - changed from 'EnsureGameMenusExist()'.
    void SmartMenuCreation()
    {
        // Only creating menu if it doesn't exist - smart caching.
        if (currentPauseMenuInstance == null)
        {
            currentPauseMenuInstance = Instantiate(pauseMenuPrefab);
            currentPauseMenuInstance.SetActive(false); // Hidden until needed by user.
            pauseMenuState = MenuState.Cached;
        }

        if (currentSettingsMenuInstance == null)
        {
            currentSettingsMenuInstance = Instantiate(settingsMenuPrefab);
            currentSettingsMenuInstance.SetActive(false); // Hidden until needed by user.
            settingsMenuState = MenuState.Cached;
        }
    }

    // Creating new menu.
    GameObject CreateNewMenu(string menuName)
    {
        // TODO: Prefab instantiation logic.
        switch (menuName)
        {
            // case "MainMenu":
            //     current = Instantiate(mainMenuPrefab);
            //     break;
            // Don't think we want to keep mainMenu in here or instantiate it in this class?
            case "PauseMenu":
                currentPauseMenuInstance = Instantiate(pauseMenuPrefab);
                return currentPauseMenuInstance;
            // break;
            // --- MOVING TUTORIAL STUFF TO ANOTHER MANAGER ---
            //case "TutorialMenu":
            //   currentMenuInstance = Instantiate(tutorialOverlayPrefab);
            //   break;
            case "SettingsMenu":
                currentSettingsMenuInstance = Instantiate(settingsMenuPrefab);
                return currentSettingsMenuInstance;
            // break;
            default:
                Debug.LogWarning("Invalid menu name: " + menuName);
                break;
        }
        Debug.Log("Created new menu: " + menuName);
        return null;
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

    // Public method to allow other systems to open menu.
    public void OpenMenu(string menuType)
    {
        switch (menuType)
        {
            case "PauseMenu":
                // Reseting cache timer since menu is being used.
                pauseMenuCacheTimer = 0f;
                pauseMenuState = MenuState.Open;
                // Telling MenuManager to show the menu.
                if (MenuManager.Instance != null)
                    MenuManager.Instance.OpenPauseMenu();
                break;
            case "SettingsMenu":
                // Reseting cache timer since settings menu is being used.
                settingsMenuCacheTimer = 0f;
                settingsMenuState = MenuState.Open;
                // TODO: Add method to tell MenuManaager to show settings.
                break;
        }
    }

    // Public method to allow other systems to close menu.
    public void CloseMenu(string menuType)
    {
        switch (menuType)
        {
            case "PauseMenu":
                pauseMenuState = MenuState.Cached; // Back to being cached - start timer.
                pauseMenuCacheTimer = 0f; // Reset timer.
                break;
            case "SettingsMenu":
                settingsMenuState = MenuState.Cached; // Back to being cached - start timer.
                settingsMenuCacheTimer = 0f; // Reset timer.
                break;
        }
    }
}
