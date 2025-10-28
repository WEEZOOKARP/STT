using UnityEngine;

// Defines WHEN and WHERE a hint should appear during gameplay.
// Focus: Enemy-type-specific hints and boss hints.
[System.Serializable]
public class HintTrigger
{
    [Header("Identification")]
    public string hintId; // Unique ID for tracking (e.g., "hint_basic_enemy", "hint_boss").

    [Header("When Should This Hint Show?")]
    public HintTriggerType triggerType;

    [Header("Trigger Data (if applicable)")]
    public string enemyTypeName; // For EnemyTypeEncountered (e.g., "Basic Enemy", "Fast Enemy").

    [Header("The Hint Content")]
    public TutorialStep associatedStep; // The hint to be shown.

    [TextArea(3, 5)]
    public string debugDescription; // e.g., "Shows when player first encounters a tank enemy".
}

public enum HintTriggerType
{
    GameStart, // Shows at game beginning.
    EnemyTypeEncountered, // Shows when player encounters specific enemy type (use enemyTypeName).
    BossEncountered, // Shows when a boss enemy spawns.
    WaveStart, // Shows when wave starts.
    BuildPhaseStart, // Shows when build phase starts.
    Custom, // Can add more later on.
}
