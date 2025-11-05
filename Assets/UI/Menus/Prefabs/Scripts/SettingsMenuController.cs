using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public static SettingsMenuController Instance { get; private set; }

    // General settings controls.
    [Header("Settings Menu Buttons")]
    public Button closeButton;
    public Button applyChangesButton;
    public Button tutorialResetButton;
    public Button tutorialOffButton;

    // Volume controls.
    [Header("Volume Controls")]
    public Slider masterVolumeSlider;
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
            closeButton.onClick.AddListener(CloseSettingsButton);

        if (applyChangesButton != null)
            applyChangesButton.onClick.AddListener(ApplyButtonClicked);

        if (tutorialResetButton != null)
            tutorialResetButton.onClick.AddListener(ResetTutorialHintsButton);

        if (tutorialOffButton != null)
            tutorialOffButton.onClick.AddListener(TurnOffTutorialHintsButton);

        if (muteSoundButton != null)
            muteSoundButton.onClick.AddListener(MuteSoundButton);

        if (masterVolumeSlider != null && SettingsManager.Instance != null)
        {
            // Setting Slider to current saved value.
            masterVolumeSlider.value = SettingsManager.Instance.currentSettings.masterVolume;
            // Listening for any changes.
            masterVolumeSlider.onValueChanged.AddListener(OnVolumeChanged);

            Debug.Log($"Volume slider intialized to: {masterVolumeSlider.value}");
        }
        // if (re != null)
        //     quitButton.onClick.AddListener(QuitGame);

        // Need to have stuff set up for slider for volume.
    }

    void OnDestroy()
    {
        // Unsubscribing from slider events to prevent memory leaks.
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }

    public void CloseSettingsButton()
    {
        Debug.Log("[SettingsMenuController] Settings close button clicked.");

        if (SettingsMenuManager.Instance != null)
            // Calling MenuManager for business logic.
            SettingsMenuManager.Instance.CloseSettingsMenu();
        // TODO: Implement an actual settings system and a save system to call.
        // Additionally, need to exit to the parent menu;
        // Furthermore, if settings have been changed and not applied yet, prompt user
        // to apply settings (or not) before exiting to parent menu.
    }

    // DISPLAY ONLY: Shows the settings menu panel.
    // NOTE: Lifecycle (state management) is handled by SettingsMenuManager!
    // Called by: MenuManager (after SettingsMenuManager updates state)
    public void ShowSettingsMenuDisplay()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[SettingsMenuController] Settings menu displayed");
        }
    }

    // DISPLAY ONLY: Hides the settings menu panel.
    // NOTE: Lifecycle (state management) is handled by SettingsMenuManager!
    // Called by: MenuManager (after SettingsMenuManager updates state)
    public void HideSettingsMenuDisplay()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("[SettingsMenuController] Settings menu hidden");
        }
    }

    public void ApplyButtonClicked()
    {
        Debug.Log("[SettingsMenuController] Apply Settings changes button clicked.");

        // Saving and applying settings when Apply button is clicked.
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ApplySettingsChanges();
            Debug.Log("Settings saved and applied successfully!");
        }
    }

    public void TurnOffTutorialHintsButton()
    {
        Debug.Log("[SettingsMenuController] Turn off tutorial hints button clicked");

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.currentSettings.tutorialEnabled = false;
            SettingsManager.Instance.ApplySettingsChanges();
        }
    }

    public void ResetTutorialHintsButton()
    {
        Debug.Log("[SettingsMenuController] Reset tutorial hints button clicked");

        if (TutorialHintTracker.Instance != null)
        {
            TutorialHintTracker.Instance.ResetAllHints();
        }
    }

    public void MuteSoundButton()
    {
        Debug.Log("[SettingsMenuController] Mute sound button clicked.");
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.MuteSound();
            Debug.Log("Muted sound successfully!");
        }
    }

    // Immediate feedback as volume is adjusted.
    public void OnVolumeChanged(float value)
    {
        // Storing old masterVolume value for debugging.
        float oldVolume = SettingsManager.Instance.currentSettings.masterVolume;

        if (SettingsManager.Instance != null)
        {
            // Updating masterVolume of game.
            SettingsManager.Instance.currentSettings.masterVolume = value;
            SettingsManager.Instance.ApplyAudioSettings(); // User hears change immediately slider dragged.

            Debug.Log($"[SettingsManager] Volume from: {oldVolume:F2} to {value:F2}");
        }
    }
}
