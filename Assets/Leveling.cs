using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public struct playerStats
{
    public int reloadMult;
    public int damageMult;
    public int healthMult;
    public int projectileLifestealMult;
}

[System.Serializable]
public struct skill {
    public String name;
    public int cost;
    public String description;
    public Boolean active;

    public skill(string name, int cost, String description, Boolean active)
    {
        this.name = name;
        this.cost = cost;
        this.description = description;
        this.active = active;
    }

    public int getCost(){
        return this.cost;
    }

    public Boolean claim()
    {
        this.active = true;
        return this.active;
    }
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
    public List<skill> skills = new List<skill>(); // The player's skills are stored here.

    void Update()
    {
        experienceBar.maxValue = Mathf.RoundToInt(experienceToNextLevel);
        experienceBar.value = Mathf.RoundToInt(experience);
        levelText.text = "Level " + level.ToString();
        SkillPointsText.text = "Skill Points: " + SkillPoints.ToString();
        levelUpButton.gameObject.SetActive(SkillPoints>=1);
    }

    public void purchaseSkill(String name) {
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].name == name && skills[i].getCost() <= SkillPoints)
            {
                SkillPoints -= skills[i].getCost();
                print(skills[i].name);
                print(skills[i].claim());
                return;
            }
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
}
