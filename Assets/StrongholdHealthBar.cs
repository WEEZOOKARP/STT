using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class StrongholdHealthBar : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [Tooltip("Main Slider that shows the current HP instantly.")]
    public UnityEngine.UI.Slider slider;

    [Tooltip("Optional second Slider that lags behind for a 'damage taken' effect.")]
    public UnityEngine.UI.Slider easeSlider;

    [Tooltip("Optional TMP label that shows 'Base 240 / 300'.")]
    public TMP_Text label;

    [Tooltip("Optional fill Image from the MAIN slider (used for color by HP%).")]
    public UnityEngine.UI.Image fillImage;

    [Header("Behaviour")]
    [Range(0.5f, 10f)]
    public float easeLerpPerSecond = 4f;

    public Color highHPColor = Color.green;
    public Color midHPColor = Color.yellow;
    public Color lowHPColor = Color.red;

    private int current;
    private int max = 1; // avoid div-by-zero
    private bool hasEase;

    void Awake()
    {
        hasEase = (easeSlider != null);

        if (slider != null)
        {
            slider.minValue = 0;
            if (slider.maxValue < 1) slider.maxValue = 1;
            if (slider.value < 0) slider.value = 0;
        }

        if (hasEase)
        {
            easeSlider.minValue = 0;
            if (easeSlider.maxValue < 1) easeSlider.maxValue = 1;
            if (easeSlider.value < 0) easeSlider.value = 0;
        }

        UpdateVisuals();
    }

    void Update()
    {
        if (!hasEase || slider == null) return;
        if (Mathf.Approximately(easeSlider.value, slider.value)) return;

        // Lerp the ease bar toward the main bar
        float step = easeLerpPerSecond * Time.deltaTime * slider.maxValue;
        easeSlider.value = Mathf.MoveTowards(easeSlider.value, slider.value, step);
    }

    /// <summary>
    /// Called by StrongholdHealth whenever HP changes.
    /// </summary>
    public void SetHealth(int current, int max)
    {
        this.current = Mathf.Max(0, current);
        this.max = Mathf.Max(1, max);

        if (slider != null)
        {
            if (!Mathf.Approximately(slider.maxValue, this.max))
                slider.maxValue = this.max;

            slider.value = this.current;
        }

        if (hasEase)
        {
            if (!Mathf.Approximately(easeSlider.maxValue, this.max))
                easeSlider.maxValue = this.max;

            // If we healed, snap ease up so it never lags upward
            if (easeSlider.value < this.current)
                easeSlider.value = this.current;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Called by StrongholdHealth when the base dies.
    /// </summary>
    public void HideOnDestroyed()
    {
        gameObject.SetActive(false);
    }

    private void UpdateVisuals()
    {
        if (label != null)
            label.text = $"Base {current} / {max}";

        if (fillImage != null && max > 0)
        {
            float pct = (float)current / max;
            if (pct > 0.6f) fillImage.color = highHPColor;
            else if (pct > 0.3f) fillImage.color = midHPColor;
            else fillImage.color = lowHPColor;
        }
    }
}
