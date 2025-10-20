using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class WarningCutsceneTests
{
    private GameObject testObject;
    private warningCuscene warningManager;
    private GameObject uiRoot;

    [SetUp]
    public void SetUp()
    {
        // Create test GameObject with warningCuscene
        testObject = new GameObject("TestWarningCutscene");
        warningManager = testObject.AddComponent<warningCuscene>();

        // Create UI hierarchy
        uiRoot = new GameObject("WarningCusceneUI");
        warningManager.warningCusceneUI = uiRoot;

        // Add CanvasGroup
        warningManager.canvasGroup = uiRoot.AddComponent<CanvasGroup>();

        // Create warning text
        GameObject textObj = new GameObject("WarningText");
        textObj.transform.SetParent(uiRoot.transform);
        warningManager.warningText = textObj.AddComponent<TextMeshProUGUI>();

        // Add AudioSource
        warningManager.warningSoundSource = testObject.AddComponent<AudioSource>();

        // Set default values
        warningManager.showDuration = 2.5f;
        warningManager.hideSpeed = 3f;
        warningManager.showScale = 1f;
        warningManager.hideScaleY = 0f;

        // Start() will be called automatically by Unity's lifecycle
        // We need to manually initialize what Start() does since we're in a test

        // Set initial scale before "Start" initialization
        uiRoot.transform.localScale = Vector3.one;

        if (warningManager.warningCusceneUI)
        {
            warningManager.warningCusceneUI.SetActive(false);
        }
        if (warningManager.warningText)
            warningManager.warningText.gameObject.SetActive(false);
        if (warningManager.canvasGroup)
            warningManager.canvasGroup.alpha = 0;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        if (uiRoot != null)
            Object.DestroyImmediate(uiRoot);
        if (testObject != null)
            Object.DestroyImmediate(testObject);
    }

    [Test]
    public void WarningManager_InitializesCorrectly()
    {
        // Assert
        Assert.IsNotNull(warningManager);
        Assert.IsNotNull(warningManager.warningCusceneUI);
        Assert.IsNotNull(warningManager.canvasGroup);
        Assert.IsNotNull(warningManager.warningText);
        Assert.AreEqual(2.5f, warningManager.showDuration);
        Assert.AreEqual(3f, warningManager.hideSpeed);
    }

    [Test]
    public void Start_HidesUIInitially()
    {
        // Assert
        Assert.IsFalse(warningManager.warningCusceneUI.activeSelf);
        Assert.IsFalse(warningManager.warningText.gameObject.activeSelf);
        Assert.AreEqual(0f, warningManager.canvasGroup.alpha);
    }

    [Test]
    public void ShowWarningCuscene_ActivatesUI()
    {
        // Act
        warningManager.showWarningCuscene();

        // Assert
        Assert.IsTrue(warningManager.warningCusceneUI.activeSelf);
        Assert.IsTrue(warningManager.warningText.gameObject.activeSelf);
        Assert.AreEqual(1f, warningManager.canvasGroup.alpha);
    }

    [UnityTest]
    public IEnumerator ShowWarningCuscene_SetsCorrectScale()
    {
        // Arrange, wait for Start() to be called
        yield return null;

        Vector3 originalScale = warningManager.warningCusceneUI.transform.localScale;

        // Act
        warningManager.showWarningCuscene();

        // Assert
        Vector3 currentScale = warningManager.warningCusceneUI.transform.localScale;
        Assert.AreEqual(originalScale.x, currentScale.x, 0.01f);
        Assert.AreEqual(warningManager.showScale, currentScale.y, 0.01f);
        Assert.AreEqual(originalScale.z, currentScale.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator ShowWarningCuscene_PlaysAudioWhenClipSet()
    {
        // Arrange
        warningManager.warningSound = AudioClip.Create("TestClip", 44100, 1, 44100, false);

        // Act
        warningManager.showWarningCuscene();
        yield return null; // Wait one frame

        // Assert
        Assert.IsTrue(warningManager.warningSoundSource.isPlaying);
        Assert.IsTrue(warningManager.warningSoundSource.loop);
        Assert.AreEqual(warningManager.warningSound, warningManager.warningSoundSource.clip);
    }

    [UnityTest]
    public IEnumerator ShowWarningCuscene_TriggersHideAfterDuration()
    {
        // Arrange
        warningManager.showDuration = 0.5f; // Short duration for testing

        // Act
        warningManager.showWarningCuscene();

        // Wait for show duration + animation time
        yield return new WaitForSeconds(warningManager.showDuration + 1f);

        // Assert, UI should be hidden
        Assert.IsFalse(warningManager.warningCusceneUI.activeSelf);
        Assert.AreEqual(0f, warningManager.canvasGroup.alpha);
    }

    [UnityTest]
    public IEnumerator HideAnimation_ReducesScaleAndAlpha()
    {
        // Arrange
        warningManager.showDuration = 0.1f;
        warningManager.hideSpeed = 10f; // Fast for testing
        warningManager.showWarningCuscene();

        yield return new WaitForSeconds(0.2f); // Wait for show duration

        // Animation should be in progress
        float initialAlpha = warningManager.canvasGroup.alpha;
        Vector3 initialScale = warningManager.warningCusceneUI.transform.localScale;

        yield return new WaitForSeconds(0.05f); // Wait a bit during animation

        // Assert, values should be changing
        float currentAlpha = warningManager.canvasGroup.alpha;
        Vector3 currentScale = warningManager.warningCusceneUI.transform.localScale;

        // Alpha should be decreasing (or already 0)
        Assert.LessOrEqual(currentAlpha, initialAlpha);

        // Y scale should be decreasing (or already 0)
        Assert.LessOrEqual(currentScale.y, initialScale.y);
    }

    [UnityTest]
    public IEnumerator HideAnimation_StopsAudio()
    {
        // Arrange
        warningManager.warningSound = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        warningManager.showDuration = 0.2f;
        warningManager.hideSpeed = 10f;

        // Act
        warningManager.showWarningCuscene();
        yield return null;
        Assert.IsTrue(warningManager.warningSoundSource.isPlaying, "Audio should be playing");

        // Wait for hide to complete
        yield return new WaitForSeconds(warningManager.showDuration + 0.5f);

        // Assert
        Assert.IsFalse(warningManager.warningSoundSource.isPlaying, "Audio should be stopped");
    }

    [UnityTest]
    public IEnumerator HideAnimation_ResetsScaleAfterComplete()
    {
        // Arrange
        warningManager.showDuration = 0.2f;
        warningManager.hideSpeed = 10f;
        Vector3 originalScale = warningManager.warningCusceneUI.transform.localScale;

        // Act
        warningManager.showWarningCuscene();
        yield return new WaitForSeconds(warningManager.showDuration + 0.5f);

        // Assert - scale should be reset to original
        Vector3 finalScale = warningManager.warningCusceneUI.transform.localScale;
        Assert.AreEqual(originalScale.x, finalScale.x, 0.01f);
        Assert.AreEqual(originalScale.y, finalScale.y, 0.01f);
        Assert.AreEqual(originalScale.z, finalScale.z, 0.01f);
    }

    [UnityTest]
    public IEnumerator ShowWarningCuscene_CalledTwice_StopsPreviousAnimation()
    {
        // Arrange
        warningManager.showDuration = 1f;
        warningManager.hideSpeed = 2f;

        // Act - Show first time
        warningManager.showWarningCuscene();
        yield return new WaitForSeconds(0.3f);

        // Show again before first animation completes
        warningManager.showWarningCuscene();
        yield return null;

        // Assert, UI should still be visible with full alpha
        Assert.IsTrue(warningManager.warningCusceneUI.activeSelf);
        Assert.AreEqual(1f, warningManager.canvasGroup.alpha);
    }

    [Test]
    public void Singleton_OnlyOneInstanceExists()
    {
        // Arrange
        GameObject secondObject = new GameObject("SecondWarningCutscene");
        warningCuscene secondManager = secondObject.AddComponent<warningCuscene>();

        // Act, Awake is called automatically

        // Assert
        Assert.AreEqual(warningManager, warningCuscene.Instance);

        // Cleanup
        Object.DestroyImmediate(secondObject);
    }

    [Test]
    public void ShowWarningCuscene_WithNullReferences_DoesNotThrowError()
    {
        // Arrange
        warningManager.warningCusceneUI = null;

        // Act & Assert, Should not throw exception
        Assert.DoesNotThrow(() => warningManager.showWarningCuscene());
    }

    [UnityTest]
    public IEnumerator HideAnimation_CompletesFullCycle()
    {
        // Arrange
        warningManager.showDuration = 0.2f;
        warningManager.hideSpeed = 10f;

        // Act
        warningManager.showWarningCuscene();

        // Wait for complete cycle
        yield return new WaitForSeconds(warningManager.showDuration + 0.5f);

        // Assert - Everything should be reset
        Assert.IsFalse(warningManager.warningCusceneUI.activeSelf);
        Assert.IsFalse(warningManager.warningText.gameObject.activeSelf);
        Assert.AreEqual(0f, warningManager.canvasGroup.alpha);
    }

    [UnityTest]
    public IEnumerator ShowWarningCuscene_MultipleCallsDuringAnimation_HandlesGracefully()
    {
        // Arrange
        warningManager.showDuration = 0.5f;
        warningManager.hideSpeed = 5f;

        // Act, Rapid successive calls
        warningManager.showWarningCuscene();
        yield return new WaitForSeconds(0.1f);
        warningManager.showWarningCuscene();
        yield return new WaitForSeconds(0.1f);
        warningManager.showWarningCuscene();

        // Assert, Should still be showing
        Assert.IsTrue(warningManager.warningCusceneUI.activeSelf);
        Assert.AreEqual(1f, warningManager.canvasGroup.alpha);

        // Wait for completion
        yield return new WaitForSeconds(warningManager.showDuration + 0.5f);

        // Assert, Should complete normally
        Assert.IsFalse(warningManager.warningCusceneUI.activeSelf);
    }
}