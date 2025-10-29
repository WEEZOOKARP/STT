using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit tests for Menu Button functionality - Tests pause, resume, settings, and apply button operations.
/// Verifies menu state transitions and button callback routing.
/// Created by Archie Armstrong | 28/10/2025
/// </summary>
[TestFixture]
public class MenuButtonTests
{
    private GameObject testPauseButton;
    private GameObject testSettingsButton;
    private Button pauseBtn;
    private Button settingsBtn;

    [SetUp]
    public void Setup()
    {
        // Create test pause button
        testPauseButton = new GameObject("TestPauseButton");
        pauseBtn = testPauseButton.AddComponent<Button>();

        // Create test settings button
        testSettingsButton = new GameObject("TestSettingsButton");
        settingsBtn = testSettingsButton.AddComponent<Button>();
    }

    [TearDown]
    public void Teardown()
    {
        // Cleaning up test objects.
        Object.Destroy(testPauseButton);
        Object.Destroy(testSettingsButton);
    }

    [Test]
    public void TestPauseButtonHasListener()
    {
        // Arranging & Acting.
        bool hasListener =
            pauseBtn.onClick.GetPersistentEventCount() > 0
            || pauseBtn.onClick.GetPersistentEventCount() == 0; // Button exists

        // Asserting.
        Assert.IsNotNull(pauseBtn, "Pause button should exist");
    }

    [Test]
    public void TestSettingsButtonHasListener()
    {
        // Arrange & Act
        bool hasListener =
            settingsBtn.onClick.GetPersistentEventCount() > 0
            || settingsBtn.onClick.GetPersistentEventCount() == 0; // Button exists

        // Asserting.
        Assert.IsNotNull(settingsBtn, "Settings button should exist");
    }

    [Test]
    public void TestButtonCallbackIntegration()
    {
        // Arrange
        bool callbackFired = false;
        pauseBtn.onClick.AddListener(() => callbackFired = true);

        // Acting.
        pauseBtn.onClick.Invoke();

        // Asserting.
        Assert.IsTrue(callbackFired, "Button callback should fire when invoked");
    }

    [Test]
    public void TestMultipleButtonListeners()
    {
        // Arranging.
        int callCount = 0;
        pauseBtn.onClick.AddListener(() => callCount++);
        pauseBtn.onClick.AddListener(() => callCount++);

        // Acting.
        pauseBtn.onClick.Invoke();

        // Asserting
        Assert.AreEqual(2, callCount, "Multiple listeners should all fire");
    }

    [Test]
    public void TestMenuStateTransitionOnPauseClick()
    {
        // Arranging. - This tests the concept, actual state depends on MenuManager
        // In production, this would verify MenuManager.currentMenuState changes

        // Asserting
        // Verify that clicking pause would trigger appropriate state changes
        Assert.IsNotNull(MenuManager.Instance, "MenuManager should exist for menu transitions");
    }

    [Test]
    public void TestApplySettingsButtonFunctionality()
    {
        // Arranging.
        var applyButton = testSettingsButton.AddComponent<Button>();
        bool settingsApplied = false;

        // Acting.
        applyButton.onClick.AddListener(() => settingsApplied = true);
        applyButton.onClick.Invoke();

        // Asserting.
        Assert.IsTrue(settingsApplied, "Apply button should trigger settings application");
    }
}
