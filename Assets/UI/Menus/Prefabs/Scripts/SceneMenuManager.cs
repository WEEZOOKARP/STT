using UnityEngine;
using UnityEngine.SceneManagement;

// Listens to scene changes and coordinates menu setup/teardown.
public class SceneMenuManager : MonoBehaviour
{
    public static SceneMenuManager Instance { get; private set; }

    private SceneNames currentScene = SceneNames.MainMenu;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneMenuManager] Scene loaded: {scene.name}");

        // Determining current scene from name.
        if (scene.name == SceneNames.CoreGameDemo.ToString())
        {
            currentScene = SceneNames.CoreGameDemo;
        }
        else if (scene.name == SceneNames.MainMenu.ToString())
        {
            currentScene = SceneNames.MainMenu;
        }

        // Route to appropriate setup based on scene.
        if (currentScene == SceneNames.CoreGameDemo)
        {
            // When GameScene loads, inform menuManager and set up menus.
            SetupGameScene();
            if (MenuManager.Instance != null)
                MenuManager.Instance.currentScene = currentScene;
        }
        else if (currentScene == SceneNames.MainMenu)
        {
            // When MainMenuScene loads, inform menuManager and set up menus.
            SetupMainMenuScene();
            if (MenuManager.Instance != null)
                MenuManager.Instance.currentScene = currentScene;
        }
        else
        {
            Debug.LogWarning($"[SceneMenuManager] Unknown scene: {scene.name}");
        }
    }

    // SCENE SETUP: Called when MainMenu loads.
    // Cleans up main menu and prepares game menus.
    public void SetupGameScene()
    {
        Debug.Log("[SceneMenuManager] Setting up Game scene menus.");

        // Destroying Main Menu for clean up.
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.DestroyMainMenu();
            Debug.Log("[SceneMenuManager] Main menu cleaned up");
        }

        // Create and cache pause menu (hidden)
        if (PauseMenuManager.Instance != null)
        {
            // Passing true to signify menu should be hidden and cached.
            PauseMenuManager.Instance.CreatePauseMenu(true);
            Debug.Log("[SceneMenuManager] Pause menu made and cached due to scene change.");
        }

        // Ensuring settings menu manager exists.
        if (SettingsMenuManager.Instance != null)
        {
            // Passing true to signify menu should be hidden and cached.
            SettingsMenuManager.Instance.CreateSettingsMenu(true);
            Debug.Log("[SceneMenuManager] Settings menu made and cached due to scene change.");
        }

        Debug.Log("[SceneMenuManager] Game scene setup complete");
    }

    // SCENE SETUP: Called when MainMenu loads.
    // Cleans up game menu and prepares main menus.
    public void SetupMainMenuScene()
    {
        Debug.Log("[SceneMenuManager] Setting up Main Menu scene");

        // Clean up game-specific menus
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.DestroyPauseMenu();
            Debug.Log("[SceneMenuManager] Pause menu cleaned up");
        }

        // Creating main menu instance, immediately visible on screen.
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.CreateMainMenu();
            Debug.Log("[SceneMenuManager] Created Main menu - main menu does not cache.");
        }

        // Ensuring settings menu manager exists.
        if (SettingsMenuManager.Instance != null)
        {
            // Creating and caching settings menu (hidden).
            SettingsMenuManager.Instance.CreateSettingsMenu(true);
            Debug.Log("[SceneMenuManager] Settings menu made and cached due to scene change.");
        }

        // Does timeScale matter?

        Debug.Log("[SceneMenuManager] Main Menu scene setup complete");
    }

    public SceneNames GetScene()
    {
        return currentScene;
    }

    // TODO: Make sure that game state is saved when player exists between scenes.
    // Not exactly a job for this class.

    // THIS IS WHY WE DON'T FUCKING PUSH RIGHT BEFORE THE DUE DATE
    // LEGIT COULDN't LOAD INTO THE DAMN GAME BECAUSE SOMEBODY FORGOT TO
    // ACTUALLY UPDATE THE SCENE TO MAKE THE PLAY BUTTON DO ANYTHING
    public void LoadCoreGameDemo()
    {
        var cam = Camera.main;
        if (cam != null) Destroy(cam.gameObject);

        SceneManager.LoadScene("CoreGameDemo", LoadSceneMode.Single);
        //SceneManager.LoadScene("music demo", LoadSceneMode.Single);
    }
}
