using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    private GameObject currentMainMenuInstance;

    [Header("MainMenu UI References")]
    public GameObject mainMenuCanvas;
    public GameObject settingsMenuCanvas; // Settings opened from MainMenu.

    [Header("MainMenu Prefab Repository")]
    public GameObject mainMenuPrefab;

    [Header("MainMenu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    void Awake()
    {
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

    // --- MainMenu-specific methods. ---

    // Shows main menu.
    public void ShowMainMenu()
    {
        if (currentMainMenuInstance != null)
        {
            currentMainMenuInstance.SetActive(true);
            Debug.Log("Should be showing MainMenu now.");
        }
    }

    // Closes main menu.
    // Usually, this could mean; The game is started, the settings is opened from mainMenu.
    public void CloseMainMenu()
    {
        if (currentMainMenuInstance != null)
        {
            currentMainMenuInstance.SetActive(false);
            Debug.Log("MainMenu should be closed now.");
        }
    }

    public void createMainMenu()
    {
        // Only creating menu if it doesn't exist.
        if (currentMainMenuInstance == null)
        {
            currentMainMenuInstance = Instantiate(mainMenuPrefab);
            // Main menu must be shown immediately on game open.
            currentMainMenuInstance.SetActive(true);
        }
    }

    public void ShowSettings()
    {
        MenuManager.Instance.OpenSettingsMenu("MainMenu");
        // Hiding the MainMenu instance while the settings menu is open.
        CloseMainMenu();
    }

    public void HideSettings()
    {
        MenuManager.Instance.CloseSettingsMenu();
    }

    // Intergration with MenuManager.
    public bool DoesMainMenuExist()
    {
        // TODO: Integrate actual checks for existance of MainMenu.
        return mainMenuCanvas != null;
    }
}
