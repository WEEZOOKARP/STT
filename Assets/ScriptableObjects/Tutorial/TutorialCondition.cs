/*
 * TutorialCondition.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 14/09/2025
 *
 * Last Updated on: ? | BY:
 * What:
 * Why:
 *
 * Purpose: Base class for the tutorial completion conditions.
 * Allows for flexible and extensible tutorial requirements.
 *
 * Dependencies: None - (Foundational class).
 *
 * Integration Points:
 * - Extended by specific condition types (e.g. Time-based, Object interaction, etc).
 * - Used by TutorialStep for completion logic.
 * - Managed by TutorialManager for state tracking.
 */

using UnityEngine;

public abstract class TutorialCondition : ScriptableObject
{
    [Header("Condition Identification")]
    public string conditionDescription;
    public bool isCompleted;

    // Polled by TutorialManager to decide when to advance.
    public abstract bool IsConditionMet();

    // Begins listening (events, timers, input).
    public abstract void StartCondition();

    // Cleans up listener(s) when step ends.
    public abstract void StopCondition();

    // Default resets completion; derived classes can extend.
    public virtual void ResetCondition()
    {
        isCompleted = false;
    }
}
