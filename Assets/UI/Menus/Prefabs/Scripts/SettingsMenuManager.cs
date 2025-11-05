using UnityEngine;

// Manages settings menu lifecycle.
// Can be created from MainMenu OR GameScene (via PauseMenu).
// NOT a persistent singleton - dies when needed or scene unloads.
// Handles cache timer interaction with MenuCacheManager.
//
// IMPORTANT: This class is LIFECYCLE ONLY - no SetActive() calls.
// Display logic belongs in SettingsMenuController.

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager Instance { get; private set; }

    [Header("Settings Menu Prefab")]
    [SerializeField]
    private GameObject settingsMenuPrefab;

    [Header("Settings Menu State")]
    private GameObject currentSettingsMenuInstance;
    private MenuStates settingsMenuState = MenuStates.Destroyed;

    // Caching logic for Settings menu.
    // Updates via references to CachingMenuManager.
    private float settingsMenuCacheTimer = 0f;
    private const float CACHE_DURATION = 30f;

    [Header("Settings Menu Parent Menu")]
    private SettingsParentMenuType settingsParent = SettingsParentMenuType.None;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No DontDestroyOnLoad - this dies with the scene or when cleaned up.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Setter for settings parent menu.
    public void SetParentMenu(SettingsParentMenuType parent)
    {
        settingsParent = parent;
    }

    // Getter for settings parent menu.
    public SettingsParentMenuType GetParentMenu() => settingsParent;

    void Update()
    {
        // Updating the cache timer.
        if (settingsMenuState == MenuStates.Cached)
        {
            MenuCachingManager.Instance.HandleCacheTimers(Time.deltaTime);
        }
    }

    // Destroying the settings menu instance.
    public void DestroySettingsMenu()
    {
        if (currentSettingsMenuInstance != null)
        {
            Destroy(currentSettingsMenuInstance);
            currentSettingsMenuInstance = null;
        }
        settingsMenuState = MenuStates.Destroyed;
        settingsMenuCacheTimer = 0f;
        Debug.Log("[SettingsMenuManager] Settings menu destroyed");
    }

    // Creates the settings menu instance.
    // bool hidden parameter - if true, menu made on scene load, if false, player clicked settings menu button.
    public void CreateSettingsMenu(bool hidden)
    {
        // If Settings Menu already exists, no need to create new.
        if (currentSettingsMenuInstance != null)
        {
            Debug.Log("[SettingsMenuManager] Settings menu already exists - skipping creation.");
            return;
        }

        // Checking prefab is assigned.
        if (settingsMenuPrefab == null)
        {
            Debug.LogError("[SettingsMenuManager] settingsMenuPrefab is not assigned!");
            return;
        }

        // Instantiating the menu.
        currentSettingsMenuInstance = Instantiate(settingsMenuPrefab);
        Debug.Log("[SettingsMenuManager] Settings menu instance created.");

        // If created due to scene change, hide display until menu required, else show.
        if (SettingsMenuController.Instance != null)
        {
            if (hidden == true)
            {
                // Hiding Settings Menu display until required.
                SettingsMenuController.Instance.HideSettingsMenuDisplay();
                CacheSettingsMenu();
                Debug.Log("[SettingsMenuManager] Settings menu hidden (cached).");
            }
            else
            {
                SettingsMenuController.Instance.ShowSettingsMenuDisplay();
                settingsMenuState = MenuStates.Open; // Stops Cache timer increasing - timer resets on menu close.
                Debug.Log("[SettingsMenuManager] Settings menu opened.");
            }
        }
    }

    // Method to Open Settings Menu
    public void OpenSettingsMenu()
    {
        CreateSettingsMenu(false);

        if (MenuManager.Instance != null)
            MenuManager.Instance.HideParentMenu(settingsParent);
    }

    // Closing settings menu when returning to parent menu.
    public void CloseSettingsMenu()
    {
        // Check to make sure settings changes are applied and not lost.
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ApplySettingsChanges();
        // Hiding the settings menu display.
        if (SettingsMenuController.Instance != null)
            SettingsMenuController.Instance.HideSettingsMenuDisplay();
        // Caching the settings Menu.
        CacheSettingsMenu();
        // Returning to the settings menu's parent menu.
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ReturnToParentMenu(settingsParent);
            Debug.Log("[SettingsMenuManager] Attempting to open parent menu through MenuManager.");
        }
    }

    // Private helper method to cache settings menu.
    private void CacheSettingsMenu()
    {
        if (currentSettingsMenuInstance == null)
        {
            Debug.LogWarning(
                "[SettingsMenuManager] Cannot cache - settings menu instance is null!"
            );
            return;
        }

        settingsMenuState = MenuStates.Cached;
        settingsMenuCacheTimer = 0f;
        Debug.Log("[SettingsMenuManager] State updated to Cached (display handled by controller).");
    }

    // Getter for the settings menu instance.
    public GameObject GetSettingsMenuInstance()
    {
        return currentSettingsMenuInstance;
    }

    // Getter for current state.
    public MenuStates GetSettingsMenuState()
    {
        return settingsMenuState;
    }

    // Getter for cache timer.
    public float GetSettingMenuCacheTimer()
    {
        return settingsMenuCacheTimer;
    }

    // Setter to let MenuCachingManager update cache timer.
    public void SetSettingsCacheTimer(float deltaTime)
    {
        settingsMenuCacheTimer += deltaTime;
    }
}
