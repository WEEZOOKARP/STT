using UnityEngine;

[CreateAssetMenu(
    fileName = "New Movement Condition",
    menuName = "Tutorial/Conditions/Movement Condition"
)]
public class MovementCondition : TutorialCondition
{
    [Header("Movement Settings")]
    [Tooltip("How long player needs to hold any movement key")]
    public float requiredHoldTime = 1f;

    [Tooltip("Show individual key hints")]
    public bool showIndividualKeys = true;

    private float movementStartTime = -1f;
    private bool hasStartedMoving = false;

    public override void StartCondition()
    {
        isCompleted = false;
        movementStartTime = -1f;
        hasStartedMoving = false;
        Debug.Log("[MovementCondition] Started - waiting for WASD input");
    }

    public override bool IsConditionMet()
    {
        // Check if any WASD key is being pressed - Added by Archie [26/09/25]
        // Purpose: Detect when player tries to move during tutorial.
        bool isMoving =
            Input.GetKey(KeyCode.W)
            || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.D);

        if (isMoving && !hasStartedMoving)
        {
            // Player just started moving.
            hasStartedMoving = true;
            movementStartTime = Time.time;
            Debug.Log("[MovementCondition] Player started moving!");
        }
        else if (!isMoving && hasStartedMoving)
        {
            // Player stopped moving, reset.
            hasStartedMoving = false;
            movementStartTime = -1f;
            Debug.Log("[MovementCondition] Player stopped moving, resetting timer");
        }

        // Check if they've been moving long enough.
        if (hasStartedMoving && Time.time - movementStartTime >= requiredHoldTime)
        {
            if (!isCompleted)
            {
                isCompleted = true;
                Debug.Log("[MovementCondition] Movement condition completed!");
            }
            return true;
        }

        return false;
    }

    public override void StopCondition()
    {
        Debug.Log("[MovementCondition] Stopped");
    }

    public override void ResetCondition()
    {
        base.ResetCondition(); // Resets isCompleted = false.
        movementStartTime = -1f;
        hasStartedMoving = false;
        Debug.Log("[MovementCondition] Reset - movement tracking reset");
    }
}
