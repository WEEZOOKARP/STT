using UnityEngine;
using UnityEngine.UI;

public class lowHpDisplay : MonoBehaviour
{
    public Image lowHpScreen;
    public healthBar playerHP;

    public float lowHpThreshold = 0.3f;  // % of max HP for "danger"
    public float maxAlpha = 0.6f;

    void Update()
    {
        if (playerHP == null || lowHpScreen == null) return;

        float hpPercent = playerHP.getHealthPercent();

        // Check if below threshold
        if (hpPercent <= lowHpThreshold)
        {
            // Interpolate alpha based on how low HP is
            float alpha = Mathf.Lerp(0f, maxAlpha, (lowHpThreshold - hpPercent) / lowHpThreshold);
            lowHpScreen.color = new Color(1f, 0f, 0f, alpha);
        }
        else
        {
            lowHpScreen.color = new Color(1f, 0f, 0f, 0f);
        }
    }
}

