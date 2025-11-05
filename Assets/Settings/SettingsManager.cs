using System; // For JSON serialization.
using System.IO; // For file operations.
using UnityEngine;

// Settings Manager.
public class SettingsManager : MonoBehaviour
{
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SettingsManager Instance { get; private set; }
    public GameSettings currentSettings;

    // JSON Save settings method.
    public void SaveSettings()
    {
        try // Error handling.
        {
            // Converting settings object to JSON string.
            string jsonData = JsonUtility.ToJson(currentSettings, true);
            Debug.Log($"JSON Data: {jsonData}"); // To see what we are saving.

            // Getting the file path.
            string filePath = Path.Combine(Application.persistentDataPath, "gameSettings.json");
            Debug.Log($"Saving to: {filePath}"); // To see where we are saving to.

            // Writing JSON to file.
            File.WriteAllText(filePath, jsonData);
            Debug.Log("[SettingsManager] Settings saved successfully!");
        }
        catch (Exception e) // If something were to go wrong.
        {
            Debug.LogError($"[SettingsManager] Failed to save settings: {e.Message}");
        }
    }

    // JSON load settings method.
    public void LoadSettings()
    {
        try // Start of error handling.
        {
            // Get the file path.
            string filePath = Path.Combine(Application.persistentDataPath, "gameSettings.json");
            Debug.Log($"Looking for file at: {filePath}"); // To see where we are looking.

            // Check if file exists.
            if (File.Exists(filePath))
            {
                Debug.Log("Settings file found!");

                // Read file content.
                string jsonData = File.ReadAllText(filePath);
                Debug.Log($"JSON Data: {jsonData}"); // See what we're loading.

                // Convert JSON to settings object.
                currentSettings = JsonUtility.FromJson<GameSettings>(jsonData);
                Debug.Log("[SettingsManager] Settings loaded succesfully!");
            }
            else
            {
                // Incase no settings have been set - E.g. users first time playing.
                Debug.Log("No settings file found - using default values.");
                currentSettings = new GameSettings(); // Creating default settings.
            }
        }
        catch (Exception e) // Issue with loading settings.
        {
            Debug.LogError($"[SettingsManager] Failed to load settings: {e.Message}");
            currentSettings = new GameSettings(); // Using default settings if load fails.
        }

        // Applying audio settings immediately, as to start game with correct volume.
        ApplyAudioSettings();
    }

    // Saving and applying changes together.
    public void ApplySettingsChanges()
    {
        SaveSettings(); // Saving settings change to file.
        ApplyAudioSettings(); // Applying to audio system immediately.
        Debug.Log("[SettingsManager] Settings changes applied.");
    }

    // Toggling mute - mutes sound instantly.
    public void MuteSound()
    {
        currentSettings.muteSound = !currentSettings.muteSound;
        ApplyAudioSettings(); // Applying mute immediately.
        Debug.Log($"[SettingsManager] Sound muted: {currentSettings.muteSound}");
    }

    public void ApplyAudioSettings()
    {
        if (currentSettings != null)
        {
            // Applying volume based on the mute state.
            AudioListener.volume = currentSettings.muteSound ? 0f : currentSettings.masterVolume;

            Debug.Log($"[SettingsManager] Audio settings applied:");
            Debug.Log($"  - Muted: {currentSettings.muteSound}");
            Debug.Log($"  - Master Volume: {currentSettings.masterVolume}");
            Debug.Log($"  - AudioListener.volume: {AudioListener.volume}");
        }
        else
        {
            Debug.LogWarning("[SettingsManager] Cannot apply audio - currentSettings is null!");
        }
    }
}
