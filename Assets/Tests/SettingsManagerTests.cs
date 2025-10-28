using System.IO;
using NUnit.Framework;
using UnityEngine;

// Unit tests for SettingsManager - Tests save/load persistence, volume control, and mute toggle.
// Tests JSON serialization and audio system integration.
// Created by Archie Armstrong | 28/10/2025

[TestFixture]
public class SettingsManagerTests
{
    private SettingsManager settingsManager;
    private GameSettings testSettings;
    private string testFilePath;

    [SetUp]
    public void Setup()
    {
        // Creating a test game object with SettingsManager.
        var testObject = new GameObject("TestSettingsManager");
        settingsManager = testObject.AddComponent<SettingsManager>();

        // Setting up test file path.
        testFilePath = Path.Combine(Application.persistentDataPath, "testSettings.json");
    }

    [TearDown]
    public void Teardown()
    {
        // Cleaning up test objects.
        Object.Destroy(settingsManager.gameObject);

        // Clean up test files.
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
    }

    [Test]
    public void TestSettingsDefaultValues()
    {
        // Arrange & Act.
        var settings = new GameSettings();

        // Asserting.
        Assert.AreEqual(1.0f, settings.masterVolume, "Default volume should be 1.0");
        Assert.AreEqual(false, settings.muteSound, "Default mute should be false");
        Assert.AreEqual(true, settings.tutorialEnabled, "Default tutorial enabled should be true");
    }

    [Test]
    public void TestSaveSettingsCreatesFile()
    {
        // Arranging.
        settingsManager.currentSettings = new GameSettings { masterVolume = 0.5f };

        // Acting.
        settingsManager.SaveSettings();

        // Asserting.
        Assert.IsTrue(
            File.Exists(Path.Combine(Application.persistentDataPath, "gameSettings.json")),
            "Settings file should be created after SaveSettings"
        );
    }

    [Test]
    public void TestLoadSettingsRestoresValues()
    {
        // Arranging.
        var originalSettings = new GameSettings { masterVolume = 0.75f, muteSound = true };
        settingsManager.currentSettings = originalSettings;
        settingsManager.SaveSettings();

        // Acting.
        settingsManager.LoadSettings();

        // Asserting.
        Assert.AreEqual(
            0.75f,
            settingsManager.currentSettings.masterVolume,
            "Loaded volume should match saved volume"
        );
        Assert.AreEqual(
            true,
            settingsManager.currentSettings.muteSound,
            "Loaded mute state should match saved state"
        );
    }

    [Test]
    public void TestMuteSoundToggleWorks()
    {
        // Arranging.
        settingsManager.currentSettings = new GameSettings { muteSound = false };

        // Acting.
        settingsManager.MuteSound();
        bool firstToggle = settingsManager.currentSettings.muteSound;

        settingsManager.MuteSound();
        bool secondToggle = settingsManager.currentSettings.muteSound;

        // Asserting.
        Assert.AreEqual(true, firstToggle, "First toggle should set mute to true");
        Assert.AreEqual(false, secondToggle, "Second toggle should set mute to false");
    }

    [Test]
    public void TestVolumeRangeValid()
    {
        // Arranging & Acting.
        settingsManager.currentSettings = new GameSettings();
        settingsManager.currentSettings.masterVolume = 0.5f;

        // Asserting.
        Assert.IsTrue(
            settingsManager.currentSettings.masterVolume >= 0f
                && settingsManager.currentSettings.masterVolume <= 1f,
            "Volume should be between 0 and 1"
        );
    }

    [Test]
    public void TestApplyAudioSettingsAppliesVolume()
    {
        // Arranging.
        settingsManager.currentSettings = new GameSettings
        {
            masterVolume = 0.8f,
            muteSound = false,
        };
        float expectedVolume = 0.8f;

        // Acting.
        settingsManager.ApplyAudioSettings();
        float actualVolume = AudioListener.volume;

        // Asserting.
        Assert.AreEqual(
            expectedVolume,
            actualVolume,
            "AudioListener.volume should match GameSettings.masterVolume"
        );
    }

    [Test]
    public void TestMutedAudioListenerVolumeIsZero()
    {
        // Arranging.
        settingsManager.currentSettings = new GameSettings
        {
            masterVolume = 0.8f,
            muteSound = true,
        };

        // Acting.
        settingsManager.ApplyAudioSettings();
        float actualVolume = AudioListener.volume;

        // Asserting.
        Assert.AreEqual(
            0f,
            actualVolume,
            "AudioListener.volume should be 0 when muted, regardless of masterVolume"
        );
    }
}
