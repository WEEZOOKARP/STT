using UnityEngine;
using UnityEngine.UI;

public class lowHpDisplay : MonoBehaviour
{
    public Image lowHpScreen;
    public healthBar playerHP;

    public float lowHpThreshold = 0.3f;
    public float maxAlpha = 0.6f;

    void Update()
    {
        bool canChange = playerHP.isInDanger();
        float hpPercent = playerHP.getHealthPercent();

        if (canChange)
        {
            float alpha = Mathf.Lerp(maxAlpha, 0f, hpPercent / lowHpThreshold);
            lowHpScreen.color = new Color(1, 0, 0, alpha);
        }
        else
        {
            lowHpScreen.color = new Color(1, 0, 0, 0);
        }
    }
}

