using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MetaProgressionData
{
    public int totalGamesPlayed = 0;
    public int totalWavesCompleted = 0;
    public int totalEnemiesKilled = 0;
    public int totalBossesKilled = 0;
    public float totalExperienceGained = 0f;
    public int highestWaveReached = 0;
    public float bestGameTime = 0f;
    
    // Permanent upgrades
    public int permanentHealthBonus = 0;
    public int permanentDamageBonus = 0;
    public float permanentSpeedBonus = 0f;
    public int permanentAmmoBonus = 0;
    public float permanentExperienceBonus = 0f;
    
    // Unlocked features
    public List<string> unlockedWeapons = new List<string>();
    public List<string> unlockedAbilities = new List<string>();
    public List<string> unlockedCosmetics = new List<string>();
    
    // Currency and resources
    public int metaCurrency = 0;
    public int skillPoints = 0;
    
    // Statistics
    public Dictionary<string, int> enemyKillCounts = new Dictionary<string, int>();
    public Dictionary<string, float> weaponUsageTime = new Dictionary<string, float>();
}

public class MetaProgression : MonoBehaviour
{
    public static MetaProgression Instance { get; private set; }
    
    [Header("Meta Progression Settings")]
    public int healthUpgradeCost = 100;
    public int damageUpgradeCost = 150;
    public int speedUpgradeCost = 200;
    public int ammoUpgradeCost = 75;
    public int experienceUpgradeCost = 300;
    
    [Header("Unlock Requirements")]
    public int wavesForWeaponUnlock = 10;
    public int bossesForAbilityUnlock = 3;
    public int killsForCosmeticUnlock = 50;
    
    private MetaProgressionData data;
    private const string SAVE_KEY = "MetaProgressionData";
    
    // Events
    public System.Action<MetaProgressionData> OnDataChanged;
    public System.Action<string> OnUpgradePurchased;
    public System.Action<string> OnFeatureUnlocked;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadData()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                data = JsonUtility.FromJson<MetaProgressionData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load meta progression data: {e.Message}");
                data = new MetaProgressionData();
            }
        }
        else
        {
            data = new MetaProgressionData();
        }
        
        // Initialize default unlocks
        InitializeDefaultUnlocks();
    }
    
    void SaveData()
    {
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        
        OnDataChanged?.Invoke(data);
    }
    
    void InitializeDefaultUnlocks()
    {
        // Add default weapon if none unlocked
        if (data.unlockedWeapons.Count == 0)
        {
            data.unlockedWeapons.Add("Basic Sword");
        }
        
        // Add default ability if none unlocked
        if (data.unlockedAbilities.Count == 0)
        {
            data.unlockedAbilities.Add("Quick Heal");
        }
    }
    
    // Game session tracking
    public void StartNewGame()
    {
        data.totalGamesPlayed++;
        SaveData();
    }
    
    public void CompleteWave(int waveNumber)
    {
        data.totalWavesCompleted++;
        if (waveNumber > data.highestWaveReached)
        {
            data.highestWaveReached = waveNumber;
        }
        SaveData();
    }
    
    public void KillEnemy(string enemyType, bool isBoss = false)
    {
        data.totalEnemiesKilled++;
        if (isBoss)
        {
            data.totalBossesKilled++;
        }
        
        // Track enemy type kills
        if (data.enemyKillCounts.ContainsKey(enemyType))
        {
            data.enemyKillCounts[enemyType]++;
        }
        else
        {
            data.enemyKillCounts[enemyType] = 1;
        }
        
        // Check for unlocks
        CheckForUnlocks();
        
        SaveData();
    }
    
    public void GainExperience(float amount)
    {
        data.totalExperienceGained += amount;
        SaveData();
    }
    
    public void EndGame(float gameTime)
    {
        if (gameTime > data.bestGameTime)
        {
            data.bestGameTime = gameTime;
        }
        SaveData();
    }
    
    // Upgrade system
    public bool CanPurchaseUpgrade(string upgradeType)
    {
        int cost = GetUpgradeCost(upgradeType);
        return data.metaCurrency >= cost;
    }
    
    public bool PurchaseUpgrade(string upgradeType)
    {
        if (!CanPurchaseUpgrade(upgradeType))
        {
            return false;
        }
        
        int cost = GetUpgradeCost(upgradeType);
        data.metaCurrency -= cost;
        
        // Apply the upgrade
        switch (upgradeType)
        {
            case "Health":
                data.permanentHealthBonus += 10;
                break;
            case "Damage":
                data.permanentDamageBonus += 5;
                break;
            case "Speed":
                data.permanentSpeedBonus += 0.5f;
                break;
            case "Ammo":
                data.permanentAmmoBonus += 5;
                break;
            case "Experience":
                data.permanentExperienceBonus += 0.1f;
                break;
        }
        
        OnUpgradePurchased?.Invoke(upgradeType);
        SaveData();
        return true;
    }
    
    int GetUpgradeCost(string upgradeType)
    {
        switch (upgradeType)
        {
            case "Health": return healthUpgradeCost;
            case "Damage": return damageUpgradeCost;
            case "Speed": return speedUpgradeCost;
            case "Ammo": return ammoUpgradeCost;
            case "Experience": return experienceUpgradeCost;
            default: return 100;
        }
    }
    
    // Unlock system
    void CheckForUnlocks()
    {
        // Weapon unlocks based on waves completed
        if (data.totalWavesCompleted >= wavesForWeaponUnlock && !data.unlockedWeapons.Contains("Advanced Rifle"))
        {
            UnlockWeapon("Advanced Rifle");
        }
        
        if (data.totalWavesCompleted >= wavesForWeaponUnlock * 2 && !data.unlockedWeapons.Contains("Plasma Cannon"))
        {
            UnlockWeapon("Plasma Cannon");
        }
        
        // Ability unlocks based on bosses killed
        if (data.totalBossesKilled >= bossesForAbilityUnlock && !data.unlockedAbilities.Contains("Shield"))
        {
            UnlockAbility("Shield");
        }
        
        if (data.totalBossesKilled >= bossesForAbilityUnlock * 2 && !data.unlockedAbilities.Contains("Time Slow"))
        {
            UnlockAbility("Time Slow");
        }
        
        // Cosmetic unlocks based on total kills
        if (data.totalEnemiesKilled >= killsForCosmeticUnlock && !data.unlockedCosmetics.Contains("Golden Armor"))
        {
            UnlockCosmetic("Golden Armor");
        }
        
        if (data.totalEnemiesKilled >= killsForCosmeticUnlock * 2 && !data.unlockedCosmetics.Contains("Particle Trail"))
        {
            UnlockCosmetic("Particle Trail");
        }
    }
    
    void UnlockWeapon(string weaponName)
    {
        if (!data.unlockedWeapons.Contains(weaponName))
        {
            data.unlockedWeapons.Add(weaponName);
            OnFeatureUnlocked?.Invoke($"Weapon: {weaponName}");
            SaveData();
        }
    }
    
    void UnlockAbility(string abilityName)
    {
        if (!data.unlockedAbilities.Contains(abilityName))
        {
            data.unlockedAbilities.Add(abilityName);
            OnFeatureUnlocked?.Invoke($"Ability: {abilityName}");
            SaveData();
        }
    }
    
    void UnlockCosmetic(string cosmeticName)
    {
        if (!data.unlockedCosmetics.Contains(cosmeticName))
        {
            data.unlockedCosmetics.Add(cosmeticName);
            OnFeatureUnlocked?.Invoke($"Cosmetic: {cosmeticName}");
            SaveData();
        }
    }
    
    // Getters for applying meta progression to gameplay
    public int GetHealthBonus() => data.permanentHealthBonus;
    public int GetDamageBonus() => data.permanentDamageBonus;
    public float GetSpeedBonus() => data.permanentSpeedBonus;
    public int GetAmmoBonus() => data.permanentAmmoBonus;
    public float GetExperienceBonus() => data.permanentExperienceBonus;
    
    public List<string> GetUnlockedWeapons() => new List<string>(data.unlockedWeapons);
    public List<string> GetUnlockedAbilities() => new List<string>(data.unlockedAbilities);
    public List<string> GetUnlockedCosmetics() => new List<string>(data.unlockedCosmetics);
    
    public MetaProgressionData GetData() => data;
    
    // Currency management
    public void AddMetaCurrency(int amount)
    {
        data.metaCurrency += amount;
        SaveData();
    }
    
    public void AddSkillPoints(int amount)
    {
        data.skillPoints += amount;
        SaveData();
    }
    
    public int GetMetaCurrency() => data.metaCurrency;
    public int GetSkillPoints() => data.skillPoints;
    
    // Reset functionality (for testing or player choice)
    public void ResetAllProgress()
    {
        data = new MetaProgressionData();
        InitializeDefaultUnlocks();
        SaveData();
        Debug.Log("Meta progression reset!");
    }
    
    // Debug methods
    [ContextMenu("Add Test Currency")]
    void AddTestCurrency()
    {
        AddMetaCurrency(1000);
    }
    
    [ContextMenu("Unlock All Features")]
    void UnlockAllFeatures()
    {
        UnlockWeapon("Advanced Rifle");
        UnlockWeapon("Plasma Cannon");
        UnlockAbility("Shield");
        UnlockAbility("Time Slow");
        UnlockCosmetic("Golden Armor");
        UnlockCosmetic("Particle Trail");
    }
}
