using UnityEngine;

[CreateAssetMenu(fileName = "New Level Up Condition", menuName = "Tutorial/Conditions/Level Up Condition")]
public class LevelUpCondition : TutorialCondition
{
    [Header("Level Requirements")]
    public int requiredLevel = 2;

    public override bool IsConditionMet()
    {
        Leveling levelingSystem = FindFirstObjectByType<Leveling>();
        return levelingSystem != null && levelingSystem.level >= requiredLevel;
    }

    public override void StartCondition()
    {
        isCompleted = false;
    }

    public override void StopCondition()
    {
        // Nothing to do here yet.
    }
}
