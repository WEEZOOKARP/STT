using UnityEngine;

[CreateAssetMenu(fileName = "New Timer Condition", menuName = "Tutorial/Conditions/Timer Condition")]
public class TimerCondition : TutorialCondition
{
    [Header("Timer Settings")]
    public float duration = 3f;
    private float startTime;
    
    [Header("Optional Input Checking")]
    [Tooltip("If true, also check for WASD input during timer")]
    public bool requireMovementInput = false;

    public override void StartCondition() {
        startTime = Time.time;
        isCompleted = false;
        Debug.Log($"[TimerCondition] Timer started at {startTime} for {duration} seconds");
    }

    public override bool IsConditionMet() {
        bool timeComplete = Time.time - startTime >= duration;
        bool inputComplete = true; // Default to true if not requiring input
        
        // If we require movement input, check for WASD
        if (requireMovementInput) {
            inputComplete = CheckMovementInput();
        }
        
        bool completed = timeComplete && inputComplete;
        if (completed && !isCompleted)
        {
            isCompleted = true;
            Debug.Log("[TimerCondition] Condition completed!");
        }
        return completed;
    }
    
    private bool CheckMovementInput() {
        // Check for WASD input - Added by Archie [26/09/25]
        // Purpose: Allow tutorial to detect if player has tried moving
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
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