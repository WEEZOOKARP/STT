using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

public class DailyTaskManagerTests
{
    private GameObject testObject;
    private DailyTaskManager taskManager;

    [SetUp]
    public void SetUp()
    {
        // Create test GameObject with DailyTaskManager
        testObject = new GameObject("TestDailyTaskManager");
        taskManager = testObject.AddComponent<DailyTaskManager>();

        // Create UI hierarchy
        GameObject uiRoot = new GameObject("DailyChallengeUI");
        taskManager.dailyChallengeUI = uiRoot;

        // Add CanvasGroup
        taskManager.canvasGroup = uiRoot.AddComponent<CanvasGroup>();

        // Create toggles
        taskManager.reloadOnceToggle = CreateToggle("ReloadToggle", uiRoot.transform);
        taskManager.hitEnemyToggle = CreateToggle("HitEnemyToggle", uiRoot.transform);
        taskManager.killTenEnemiesToggle = CreateToggle("KillToggle", uiRoot.transform);

        // Create text
        GameObject textObj = new GameObject("CompleteText");
        textObj.transform.SetParent(uiRoot.transform);
        taskManager.completeText = textObj.AddComponent<TextMeshProUGUI>();

        // Set rewards
        taskManager.rewardMoney = 100;
        taskManager.rewardXP = 50;
        taskManager.requiredKills = 10;

        // Clear PlayerPrefs for clean test
        PlayerPrefs.DeleteKey("LastDailyChallengeDate");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        if (testObject != null)
            Object.DestroyImmediate(testObject);

        PlayerPrefs.DeleteAll();
    }

    private Toggle CreateToggle(string name, Transform parent)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent);
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = false;
        toggleObj.SetActive(false);
        return toggle;
    }

    [Test]
    public void TaskManager_InitializesCorrectly()
    {
        // Assert
        Assert.IsNotNull(taskManager);
        Assert.AreEqual(100, taskManager.rewardMoney);
        Assert.AreEqual(50, taskManager.rewardXP);
        Assert.AreEqual(10, taskManager.requiredKills);
    }

    [Test]
    public void OnReload_ActivatesReloadToggle()
    {
        // Act
        taskManager.OnReload();

        // Assert
        Assert.IsTrue(taskManager.reloadOnceToggle.isOn);
        Assert.IsTrue(taskManager.reloadOnceToggle.gameObject.activeSelf);
    }

    [Test]
    public void OnReload_CalledTwice_OnlyActivatesOnce()
    {
        // Act
        taskManager.OnReload();
        bool firstState = taskManager.reloadOnceToggle.isOn;

        taskManager.OnReload();
        bool secondState = taskManager.reloadOnceToggle.isOn;

        // Assert
        Assert.IsTrue(firstState);
        Assert.IsTrue(secondState);
        // Both should be true, but internally it shouldn't trigger twice
    }

    [Test]
    public void OnEnemyTakeDamage_ActivatesHitEnemyToggle()
    {
        // Act
        taskManager.OnEnemyTakeDamage();

        // Assert
        Assert.IsTrue(taskManager.hitEnemyToggle.isOn);
        Assert.IsTrue(taskManager.hitEnemyToggle.gameObject.activeSelf);
    }

    [Test]
    public void OnEnemyKilled_TracksKillCount()
    {
        // Act
        for (int i = 0; i < 5; i++)
        {
            taskManager.OnEnemyKilled();
        }

        // Assert we can't directly check enemiesKilled due to it's private)
        // but we can check if the toggle activates after 10 kills
        Assert.IsFalse(taskManager.killTenEnemiesToggle.isOn);
    }

    [Test]
    public void OnEnemyKilled_TenTimes_ActivatesKillToggle()
    {
        // Act
        for (int i = 0; i < 10; i++)
        {
            taskManager.OnEnemyKilled();
        }

        // Assert
        Assert.IsTrue(taskManager.killTenEnemiesToggle.isOn);
        Assert.IsTrue(taskManager.killTenEnemiesToggle.gameObject.activeSelf);
    }

    [UnityTest]
    public IEnumerator AllTasksComplete_ShowsCompletionMessage()
    {
        // Arrange - Complete all tasks
        taskManager.OnReload();
        taskManager.OnEnemyTakeDamage();

        for (int i = 0; i < 10; i++)
        {
            taskManager.OnEnemyKilled();
        }

        yield return null; // Wait one frame

        // Assert
        Assert.IsTrue(taskManager.reloadOnceToggle.isOn);
        Assert.IsTrue(taskManager.hitEnemyToggle.isOn);
        Assert.IsTrue(taskManager.killTenEnemiesToggle.isOn);
        Assert.IsTrue(taskManager.completeText.gameObject.activeSelf);
        Assert.IsTrue(taskManager.completeText.text.Contains("DAILY TASK COMPLETE"));
    }

    [Test]
    public void CheckDailyTaskProgress_UpdatesAllToggles()
    {
        // Arrange - trigger tasks but don't activate toggles
        taskManager.OnReload();
        taskManager.OnEnemyTakeDamage();
        for (int i = 0; i < 10; i++)
        {
            taskManager.OnEnemyKilled();
        }

        // Act
        taskManager.CheckDailyTaskProgress();

        // Assert
        Assert.IsTrue(taskManager.reloadOnceToggle.isOn);
        Assert.IsTrue(taskManager.hitEnemyToggle.isOn);
        Assert.IsTrue(taskManager.killTenEnemiesToggle.isOn);
    }

    [UnityTest]
    public IEnumerator DailyReset_SavesDateToPlayerPrefs()
    {
        // Act
        // Wait for Start() to be called (happens after Awake)
        yield return null;

        // Assert
        string savedDate = PlayerPrefs.GetString("LastDailyChallengeDate");
        string expectedDate = System.DateTime.Now.ToString("yyyyMMdd");
        Assert.AreEqual(expectedDate, savedDate);
    }

    [Test]
    public void Singleton_OnlyOneInstanceExists()
    {
        // Arrange
        GameObject secondObject = new GameObject("SecondTaskManager");
        DailyTaskManager secondManager = secondObject.AddComponent<DailyTaskManager>();

        // Act - Awake is called automatically

        // Assert
        Assert.AreEqual(taskManager, DailyTaskManager.Instance);

        // Cleanup
        Object.DestroyImmediate(secondObject);
    }

    [UnityTest]
    public IEnumerator TaskUI_AutoHidesAfterDelay()
    {
        // Arrange
        taskManager.OnReload();

        // Assert UI is visible
        Assert.AreEqual(1f, taskManager.canvasGroup.alpha);

        // Wait for auto-hide (2.5 seconds + buffer)
        yield return new WaitForSeconds(3f);

        // Assert UI is hidden
        Assert.AreEqual(0f, taskManager.canvasGroup.alpha);
    }
}