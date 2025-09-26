using UnityEngine;

[CreateAssetMenu(
    fileName = "New Reload Condition",
    menuName = "Tutorial/Conditions/Reload Condition"
)]
public class ReloadCondition : TutorialCondition
{
    [Header("Reload Requirements")]
    public int requiredReloads = 1;
    public int currentReloads = 0;

    public override bool IsConditionMet()
    {
        return currentReloads >= requiredReloads;
    }

    public override void StartCondition()
    {
        currentReloads = 0;
        isCompleted = false;
        GunController gunController = FindObjectOfType<GunController>();
        if (gunController != null)
        {
            GunController.OnReloadStarted += OnReloadPerformed;
        }
    }

    public override void StopCondition()
    {
        GunController gunController = FindObjectOfType<GunController>();
        if (gunController != null)
        {
            GunController.OnReloadStarted -= OnReloadPerformed;
        }
    }

    public override void ResetCondition()
    {
        base.ResetCondition(); // Resets isCompleted = false.
        currentReloads = 0; // Reset reload count for next tutorial run.
        Debug.Log("[ReloadCondition] Reset - currentReloads set to 0");
    }

    private void OnReloadPerformed()
    {
        currentReloads++;
        if (IsConditionMet())
        {
            isCompleted = true;
        }
    }
}
