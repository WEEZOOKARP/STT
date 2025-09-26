using UnityEngine;

[CreateAssetMenu(fileName = "New Timer Condition", menuName = "Tutorial/Conditions/Timer Condition")]
public class TimerCondition : TutorialCondition
{
    [Header("Timer Settings")]
    public float duration = 3f;
    private float startTime;

    public override void StartCondition() {
        startTime = Time.time;
        isCompleted = false;
        Debug.Log($"[TimerCondition] Timer started at {startTime} for {duration} seconds");
    }

    public override bool IsConditionMet() {
        bool completed = Time.time - startTime >= duration;
        if (completed && !isCompleted)
        {
            isCompleted = true;
            Debug.Log("[TimerCondition] Condition completed!");
        }
        return completed;
    }

    public override void ResetCondition() {
        base.ResetCondition(); // Resets isCompleted = false.
        startTime = 0f; // Reset start time for next tutorial run.
        Debug.Log("[TimerCondition] Reset - startTime set to 0");
    }

    public override void StopCondition() {
        Debug.Log("[TimerCondition] Stopped");
    }
}