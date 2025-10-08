using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DailyTaskManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dailyChallengeUI;
    public Toggle noMissBulletToggle;
    public Toggle noDamageToggle;
    public Toggle killEnemiesToggle;
    public CanvasGroup canvasGroup;

    [Header("Rewards")]
    public int rewardMoney = 100;
    public int rewardXP = 50;

    private bool playedOnce = false;
    private bool noDamageTaken = false;
    private bool noMissBullet=false;
    private bool rewardGiven = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
