using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public static SettingsMenuController Instance { get; private set; }

    [Header("Settings Menu Buttons")]
    public Button closeButton;
    public Button applyChangesButton;
    public Button tutorialResetButton;
    public Button tutorialOffButton;
    public Button muteSoundButton;

    // Need to pair game sound to the core sound settings of the phone.
    // Need to also have the sound as a slider perhaps?

    [Header("Menu Panels")]
    public GameObject settingsPanel = null;

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

    void Start()
    {
        // Wiring up button events.
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);
        if (applyChangesButton != null)
            applyChangesButton.onClick.AddListener(ApplySettingsChanges);
        if (tutorialResetButton != null)
            tutorialResetButton.onClick.AddListener(ResetTutorialHints);
        if (tutorialOffButton != null)
            tutorialOffButton.onClick.AddListener(TurnOffTutorialHints);
        if (muteSoundButton != null)
            muteSoundButton.onClick.AddListener(MuteSound);
        // if (re != null)
        //     quitButton.onClick.AddListener(QuitGame);

        // Need to have stuff set up for slider for volume.
    }

    public void CloseSettings()
    {
        Debug.Log("[SettingsMenuController] Settings close button clicked.");

        // Calling MenuManager for business logic.
        MenuManager.Instance.CloseSettingsMenu();

        // Controller calls helper method to handle display close.
        closeSettingsDisplay();
        // TODO: Implement an actual settings system and a save system to call.
        // Additionally, need to exit to the parent menu;
        // Furthermore, if settings have been changed and not applied yet, prompt user
        // to apply settings (or not) before exiting to parent menu.
    }

    public void OpenSettingsMenu() 
    {
        if (settingsPanel != null) {
            openSettingsDisplay();
        }
    }

    // Private helper method to open settings display.
    public void openSettingsDisplay() {
        settingsPanel.SetActive(true);
    }

    // Private helper method to close settings display.
    public void closeSettingsDisplay()
    {
        settingsPanel.SetActive(false);
    }

    public void TurnOffTutorialHints()
    {
        Debug.Log("[SettingsMenuController] Turning off future tutorial hints.");
        // Link to tutorial system.
    }

    public void ResetTutorialHints()
    {
        Debug.Log("[SettingsMenuController] Reseting tutorial hints progress.");
        // Link to tutorial system.
    }

    public void ApplySettingsChanges()
    {
        Debug.Log("[SettingsMenuController] Apply Settings changes button clicked.");
        // TODO: Implement so that settings changes are saved, applied and reflected in
        // SettingsMenu's UI. Importantly, this does not
    }

    public void MuteSound()
    {
        Debug.Log("[SettingsMenuController] Mute sound button clicked.");
    }
}
