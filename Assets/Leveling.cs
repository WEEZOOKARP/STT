using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[Serializable]
public class PlayerStats
{
    public int healthMult = 1; // increase health
    public int damageMult = 1; // increase damage
    public int reloadMult = 1; // reload speed multiplier
    public int projectileLifestealMult = 0; // when you hit an enemy you heal
}

[Serializable]
public class Skill
{
    public string Name;
    public int Cost;
    public string Description;
    public bool IsUnlocked;
    public string Affects; // e.g. "damage", "health", "reload", etc.

    public Skill(string name, int cost, string description, string affects)
    {
        Name = name;
        Cost = cost;
        Description = description;
        Affects = affects;
        IsUnlocked = false;
    }

    public bool CanUnlock(int availablePoints)
    {
        return !IsUnlocked && availablePoints >= Cost;
    }

    public void Unlock()
    {
        IsUnlocked = true;
    }
}

public class Leveling : MonoBehaviour
{
    [Header("Player Stats")]
    public float experience = 0f; // stores player's current experience
    public int level = 1; // stores player's current level
    private float experienceToNextLevel = 100f; // how much xp in a level
    public int incrimentPerLevel = 50; // how much xp to go up per level
    public int SkillPoints = 0; // stores player's skill points
    public PlayerStats playerStats = new PlayerStats();

    [Header("UI")]
    public Slider experienceBar; // shows player's experience
    public Button levelUpButton; // button to level up
    public TextMeshProUGUI levelText; // shows player's level
    public TextMeshProUGUI SkillPointsText; // shows player's skill points

    [Header("Skills")]
    public List<Skill> skills = new List<Skill>();

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        experienceBar.maxValue = Mathf.RoundToInt(experienceToNextLevel);
        experienceBar.value = Mathf.RoundToInt(experience);
        levelText.text = "Level " + level.ToString();
        SkillPointsText.text = "Skill Points: " + SkillPoints.ToString();
        levelUpButton.gameObject.SetActive(SkillPoints>=1);
    }

    public void PurchaseSkill(string name)
    {
        Skill skill = skills.Find(s => s.Name == name);

        if (skill != null && skill.CanUnlock(SkillPoints))
        {
            SkillPoints -= skill.Cost;
            skill.Unlock();
            ApplySkillEffect(skill);
            Debug.Log($"Unlocked skill: {skill.Name}");
        }
        else
        {
            Debug.LogWarning($"Cannot unlock skill: {name}");
        }
    }

    // reset xp, increase level, increased xp needed, give skill point
    public void LevelUp()
    {
        experience = 0f;
        level++;
        experienceToNextLevel += incrimentPerLevel;
        SkillPoints++;
    }

    // add xp and check for level up
    public void AddExperience(float amount)
    {
        // Apply meta progression experience bonus
        float bonusMultiplier = 1f;
        if (MetaProgression.Instance != null)
        {
            bonusMultiplier += MetaProgression.Instance.GetExperienceBonus();
        }
        
        float finalAmount = amount * bonusMultiplier;
        experience += finalAmount;
        
        // Track experience gain in meta progression
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.GainExperience(finalAmount);
        }
        
        if (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void ApplySkillEffect(Skill skill)
    {
        switch (skill.Affects.ToLower())
        {
            case "damage":
                playerStats.damageMult++;
                break;
            case "health":
                playerStats.healthMult++;
                break;
            case "reload":
                playerStats.reloadMult++;
                break;
            case "lifesteal":
                playerStats.projectileLifestealMult++;
                break;
            default:
                Debug.LogWarning($"Skill effect not recognized: {skill.Affects}");
                break;
        }
    }
}
