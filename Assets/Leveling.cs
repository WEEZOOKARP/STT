using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct playerStats
{
    public int reloadMult;
    public int damageMult;
    public int healthMult;
    public int projectileLifestealMult;
}

public class Leveling : MonoBehaviour
{
    public float experience = 0f; // stores player's current experience
    public int level = 1; // stores player's current level
    private float experienceToNextLevel = 100f; // how much xp in a level
    public int incrimentPerLevel = 50; // how much xp to go up per level
    public Slider experienceBar; // shows player's experience
    public TextMeshProUGUI levelText; // shows player's level
    public TextMeshProUGUI SkillPointsText; // shows player's skill points
    public Button levelUpButton; // button to level up
    public int SkillPoints = 0; // stores player's skill points
    
    void Update()
    {
        experienceBar.maxValue = Mathf.RoundToInt(experienceToNextLevel);
        experienceBar.value = Mathf.RoundToInt(experience);
        levelText.text = "Level " + level.ToString();
        SkillPointsText.text = "Skill Points: " + SkillPoints.ToString();
        levelUpButton.gameObject.SetActive(SkillPoints>=1);
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
}
