using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Leveling : MonoBehaviour
{
    public float experience = 0f; // stores player's current experience
    public int level = 1; // stores player's current level
    private float experienceToNextLevel = 100f; // how much xp in a level
    public int incrimentPerLevel = 50; // how much xp to go up per level
    public Slider experienceBar; // shows player's experience
    public TextMeshProUGUI levelText; // shows player's level
    public TextMeshProUGUI levelUpPointsText; // shows player's skill points
    public Button levelUpButton; // button to level up
    public int levelUpPoints = 0; // stores player's skill points
    private bool canUpskill = false; // toggles display of level up button

    void Update()
    {
        experienceBar.value = Mathf.RoundToInt(experience);
        levelText.text = "Level " + level.ToString();
        if (levelUpPoints <= 0){
            levelUpPointsText.text = "Skill Points: " + levelUpPoints.ToString();
        } else {
            levelUpPointsText.text = "";
        }
        levelUpButton.enabled = canUpskill;
    }

    // reset xp, increase level, increased xp needed, give skill point
    public void LevelUp()
    {
        experience = 0f;
        level++;
        experienceToNextLevel += incrimentPerLevel;
        levelUpPoints++;
    }

    // add xp and check for level up
    public void AddExperience(float amount)
    {
        experience += amount;
        if (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }
}
