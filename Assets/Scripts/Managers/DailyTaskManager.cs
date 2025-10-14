using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyTaskManager : MonoBehaviour
{
    public static DailyTaskManager Instance;

    [Header("UI References")]
    public GameObject dailyChallengeUI;
    public CanvasGroup canvasGroup;
    public Toggle reloadOnceToggle;
    public Toggle hitEnemyToggle;
    public Toggle killTenEnemiesToggle;
    public TMP_Text completeText;

    [Header("Rewards")]
    public int rewardMoney = 100;
    public int rewardXP = 50;

    [Header("Settings")]
    public int requiredKills = 10;

    private bool reloadOnce = false;
    private bool hitEnemy = false;
    private int enemiesKilled = 0;
    private bool rewardGiven = false;

    private const string LastDailyKey = "LastDailyChallengeDate";

    private void Awake()
    {
        // Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CheckDailyReset();

        // Hide all toggles and UI at start
        if (reloadOnceToggle) reloadOnceToggle.gameObject.SetActive(false);
        if (hitEnemyToggle) hitEnemyToggle.gameObject.SetActive(false);
        if (killTenEnemiesToggle) killTenEnemiesToggle.gameObject.SetActive(false);
        if (completeText) completeText.gameObject.SetActive(false);

        if (dailyChallengeUI) dailyChallengeUI.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0;
    }

    // Called when player starts/enters a game
    public void OnReload()
    {
        if (!reloadOnce)
        {
            reloadOnce = true;
            ActivateToggle(reloadOnceToggle, "Reload Once!");
            CheckCompletion();
        }
    }

    // Called when enemy takes damage
    public void OnEnemyTakeDamage()
    {
        if (!hitEnemy)
        {
            hitEnemy = true;
            ActivateToggle(hitEnemyToggle, "Hit an enemy!");
            CheckCompletion();
        }
    }

    // Called when enemy dies
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"Enemy killed! Current count: {enemiesKilled}/{requiredKills}");

        if (enemiesKilled == requiredKills)
        {
            ActivateToggle(killTenEnemiesToggle, "Killed 10 enemies!");
            CheckCompletion();
        }
    }

    // Called when a wave ends
    public void OnWavesCompleted()
    {
        CheckDailyTaskProgress();
    }

    // Check all tasks status
    public void CheckDailyTaskProgress()
    {
        if (reloadOnce && !reloadOnceToggle.isOn)
            ActivateToggle(reloadOnceToggle, "Reload Once!");

        if (hitEnemy && !hitEnemyToggle.isOn)
            ActivateToggle(hitEnemyToggle, "Hit an enemy!");

        if (enemiesKilled >= requiredKills && !killTenEnemiesToggle.isOn)
            ActivateToggle(killTenEnemiesToggle, "Killed 10 enemies!");

        CheckCompletion();
    }

    // Helper to activate a toggle and show UI
    private void ActivateToggle(Toggle toggle, string message)
    {
        if (toggle == null) return;
        toggle.isOn = true;
        toggle.gameObject.SetActive(true);
        ShowTaskUI(message);
        Debug.Log($"[DailyTask] {message}");
    }

    // Shows popup message
    private void ShowTaskUI(string message)
    {
        if (dailyChallengeUI == null || canvasGroup == null || completeText == null)
            return;

        dailyChallengeUI.SetActive(true);
        canvasGroup.alpha = 1;
        completeText.text = message;
        completeText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideTaskUI));
        Invoke(nameof(HideTaskUI), 2.5f); // auto-hide
    }

    private void HideTaskUI()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0;
        if (dailyChallengeUI != null)
            dailyChallengeUI.SetActive(false);
    }

    // Check if all tasks are complete
    private void CheckCompletion()
    {
        if (rewardGiven) return;

        bool allComplete = reloadOnce && hitEnemy && enemiesKilled >= requiredKills;

        if (allComplete)
        {
            rewardGiven = true;
            ShowTaskUI("DAILY TASK COMPLETE!\n +$100\n +XP50");
            Debug.Log("Daily Challenge Complete! +50 XP +$100");

           // Add gold using GoldService (handles fallback if MetaProgression not ready)
           if (GoldService.Instance != null)
           {
                 GoldService.Instance.Add(rewardMoney);
           }
           else if (MetaProgression.Instance != null)
           {
                 // fallback (shouldn’t happen often)
                 MetaProgression.Instance.AddMetaCurrency(rewardMoney);
           }

           // Award XP
           Leveling lvl = FindObjectOfType<Leveling>();
           if (lvl != null)
           {
               lvl.AddExperience(rewardXP);
               Debug.Log($"[DailyTask] Awarded {rewardXP} XP to player.");
           }
           else if (MetaProgression.Instance != null)
           {
               MetaProgression.Instance.GainExperience(rewardXP);
               Debug.Log($"[DailyTask] Awarded {rewardXP} XP via MetaProgression.");
           }
           else
           {
              Debug.LogWarning("[DailyTask] No Leveling or MetaProgression found for XP reward!");
           }

        }
    }

    private void CheckDailyReset()
    {
        string today = System.DateTime.Now.ToString("yyyyMMdd");
        string lastDate = PlayerPrefs.GetString(LastDailyKey, "");

        if (lastDate != today)
        {
            ResetDailyChallenge();
            PlayerPrefs.SetString(LastDailyKey, today);
            PlayerPrefs.Save();
            Debug.Log("New daily challenge generated for " + today);
        }
    }

    private void ResetDailyChallenge()
    {
        reloadOnce = false;
        hitEnemy = false;
        enemiesKilled = 0;
        rewardGiven = false;

        if (reloadOnceToggle) { reloadOnceToggle.isOn = false; reloadOnceToggle.gameObject.SetActive(false); }
        if (hitEnemyToggle) { hitEnemyToggle.isOn = false; hitEnemyToggle.gameObject.SetActive(false); }
        if (killTenEnemiesToggle) { killTenEnemiesToggle.isOn = false; killTenEnemiesToggle.gameObject.SetActive(false); }

        if (dailyChallengeUI) dailyChallengeUI.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0;
        if (completeText) completeText.gameObject.SetActive(false);
    }
}
