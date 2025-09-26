/*
 * ShootCondition.cs
 *
 * Created by Archie Armstrong | 21155564
 * Date: 14/09/2025
 *
 * Last Updated on: ? | BY:
 * What:
 * Why:
 *
 * Purpose: Condition that completes when player shoots required number of times.
 *
 * Dependencies: TutorialCondition - (Base class).
 *
 * Integration Points:
 * - Used by TutorialStep for shooting tutorial steps.
 * - Monitors gunController for shot events.
 * - Provides feedback to TutorialManager.
 */

using UnityEngine;

[CreateAssetMenu(
    fileName = "New Shoot Condition",
    menuName = "Tutorial/Conditions/Shoot Condition"
)]
public class ShootCondition : TutorialCondition
{
    [Header("Shoot Requirements")]
    public int requiredShots = 1;
    public int currentShots = 0;

    [Header("References")]
    public GunController gunController;

    // Called by TutorialManager when this step begins.
    // Why: start listening here so we don't handle shots outside of the step's lifetime.
    public override void StartCondition()
    {
        currentShots = 0;
        isCompleted = false;

        // Subscribe to gunController events.
        GunController.OnShotFired += OnShotFired;
        Debug.Log("[ShootCondition] Subscribed to OnShotFired event");
    }

    // Called by TutorialManager when the step completes or tutorial ends.
    // Why: must unsubscribe to avoid memory leaks and ghost listeners in later steos.
    public override void StopCondition()
    {
        // Unsubscribe from events.
        GunController.OnShotFired -= OnShotFired;
        Debug.Log("[ShootCondition] Unsubscribed from OnShotFired event");
    }

    // Private handler receives the event and updates progress.
    private void OnShotFired()
    {
        currentShots++;
        Debug.Log($"[ShootCondition] Shot fired! Current shots: {currentShots}/{requiredShots}");

        if (currentShots >= requiredShots)
        {
            isCompleted = true; // IsConditionMet() will start returning true.
            Debug.Log("[ShootCondition] Condition completed!");
        }
    }

    public override void ResetCondition()
    {
        base.ResetCondition(); // Resets isCompleted = false
        currentShots = 0; // Reset shot count for next tutorial run
        Debug.Log("[ShootCondition] Reset - currentShots set to 0");
    }

    // Polled by TutorialManager each frame.
    public override bool IsConditionMet()
    {
        return currentShots >= requiredShots;
    }
}
