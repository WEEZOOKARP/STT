using UnityEngine;

[CreateAssetMenu(
    fileName = "New Survive Wave Condition",
    menuName = "Tutorial/Conditions/Survive Wave Condition"
)]
public class SurviveWaveCondition : TutorialCondition
{
    [Header("Survive Wave Requirements")]
    public int requiredWaves = 1;
    public int currentWaves = 0;

    public override bool IsConditionMet()
    {
        return currentWaves >= requiredWaves;
    }

    public override void StartCondition()
    {
        currentWaves = 0;
        isCompleted = false;

        // Using existing WaveManager events - subscribing to wave complete event.
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnWaveComplete += OnWaveComplete;
        }
    }

    public override void StopCondition() {
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        // Unsubscribing from wave complete event.
        if (waveManager != null) {
            waveManager.OnWaveComplete -= OnWaveComplete;
        }
     }

    // Private handler receives the event and updates progress.
    private void OnWaveComplete(int waveNumber) {
        currentWaves++;
        if (currentWaves >= requiredWaves) {
            isCompleted = true;
        }
    }
}
