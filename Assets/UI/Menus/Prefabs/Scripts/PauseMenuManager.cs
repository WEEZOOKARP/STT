using UnityEngine;

// Manages pause menu lifecycle for GameScene only.
// NOT a persistent singleton - dies when scene unloads.
// Handles cache timer interaction with MenuCacheManager.
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("Pause Menu Prefab")]
    [SerializeField]
    private GameObject pauseMenuPrefab;

    [Header("Pause Menu Settings")]
    private GameObject currentPauseMenuInstance;
    private MenuStates pauseMenuState = MenuStates.Destroyed;

    // Caching logic for Pause Menu - Updated using MenuCachingManager references.
    private float pauseMenuCacheTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad, dies with scene.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Creates/Instaniates the pause menu.
    // bool hidden parameter - if true, menu made on scene load, if false, player clicked pause menu button.
    public void CreatePauseMenu(bool hidden)
    {
        if (pauseMenuState == MenuStates.Cached)
        {
            pauseMenuState = MenuStates.Open; // Stops Cache timer increasing - timer resets on menu close.
            OpenPauseMenu();
        }
        
        // If pauseMenu already exists, no need to create new.
        if (currentPauseMenuInstance != null)
        {
            Debug.Log("[PauseMenuManager] Pause menu already exists - skipping creation.");
            return;
        }

        // Checking prefab is assigned.
        if (pauseMenuPrefab == null)
        {
            Debug.LogError("[PauseMenuManager] pauseMenuPrefab is not assigned!");
            return;
        }

        // Instantiating the menu.
        currentPauseMenuInstance = Instantiate(pauseMenuPrefab);
        Debug.Log("[PauseMenuManager] Pause menu instance created.");

        // If created due to scene change, hide display until menu required, else show.
        if (PauseMenuController.Instance != null)
        {
            if (hidden == true)
            {
                // Updating state to Cached.
                PauseMenuController.Instance.HidePauseMenuDisplay();
                CachePauseMenu();
                Debug.Log("[PauseMenuManager] Pause menu hidden (cached).");
            }
            else
            {
                // Opening the pause menu display.
                PauseMenuController.Instance.ShowPauseMenuDisplay();
                pauseMenuState = MenuStates.Open; // Stops Cache timer increasing - timer resets on menu close.
                Debug.Log("[PauseMenuManager] Pause menu opened.");
            }
        }
    }

    void Update()
    {
        // Updating the cache timer.
        if (pauseMenuState == MenuStates.Cached)
        {
            MenuCachingManager.Instance.HandleCacheTimers(Time.deltaTime);
        }
    }

    // Destroying the pause menu instance.
    public void DestroyPauseMenu()
    {
        if (currentPauseMenuInstance != null)
        {
            Destroy(currentPauseMenuInstance);
            currentPauseMenuInstance = null;
        }
        pauseMenuState = MenuStates.Destroyed;
        pauseMenuCacheTimer = 0f; // Redundant?
        Debug.Log("[PauseMenuManager] Pause menu destroyed");
    }

    // Opening pause menu.
    public void OpenPauseMenu()
    {
        CreatePauseMenu(false);
        if (PauseMenuController.Instance != null)
            PauseMenuController.Instance.ShowPauseMenuDisplay();

        // Setting pause reason.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.MenuOpen);
        }

        // Pausing time.
        Time.timeScale = 0f;
    }

    // Closing and caching pause menu when returning to game.
    public void ClosePauseMenu()
    {
        if (PauseMenuController.Instance != null)
            PauseMenuController.Instance.HidePauseMenuDisplay();

        // Unpausing time.
        Time.timeScale = 1f;

        // Clearing pause reason.
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseReason(GamePauseReason.NotPaused);
        }

        // Caching pause menu.
        CachePauseMenu();
    }

    // Helper method to cache pause menu.
    public void CachePauseMenu()
    {
        if (currentPauseMenuInstance == null)
        {
            Debug.LogWarning("[PauseMenuManager] Cannot cache - pause menu instance is null!");
            return;
        }

        pauseMenuState = MenuStates.Cached;
        pauseMenuCacheTimer = 0f;
        Debug.Log("[PauseMenuManager] State updated to Cached (display handled by controller)");
    }

    // Method to call for opening settings menu.
    public void OpenSettings()
    {
        // Telling MenuManager to determine which menu is the parent (PauseMenu in this case).
        if (MenuManager.Instance != null)
            MenuManager.Instance.FindSettingsParentMenu();
        
        // Then opening settings, which will hide this PauseMenu. 
        if (SettingsMenuManager.Instance != null)
            SettingsMenuManager.Instance.OpenSettingsMenu();
    }

    // Getter for the pause menu instance (needed by MenuManager to pass to controller)
    public GameObject GetPauseMenuInstance()
    {
        return currentPauseMenuInstance;
    }

    // Getter for menu state.
    public MenuStates GetPauseMenuState()
    {
        return pauseMenuState;
    }

    // Getter for cache timer.
    public float GetPauseMenuCacheTimer()
    {
        return pauseMenuCacheTimer;
    }

    // Setter to let MenuCachingManager update cache timer.
    public void SetPauseMenuCacheTimer(float deltaTime)
    {
        pauseMenuCacheTimer += deltaTime;
    }
}
