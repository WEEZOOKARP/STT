using UnityEngine;

// Settings data class.
[System.Serializable]
public class GameSettings
{
    // Volume controls.
    public float masterVolume = 1.0f;
    public bool muteSound = false;

    // Tutorial controls.
    public bool tutorialEnabled = true;

    // TODO: Graphics quality implementation.
    public int graphicsQuality = 2;

    // Game difficulty controls.
    public DifficultySetting currentDifficulty = DifficultySetting.Normal;

    public enum DifficultySetting
    {
        Chaos,
        Hard,
        Normal,
        Easy,
    }

    public void TurnSoundOff()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.MuteSound();
        }
    }
}
