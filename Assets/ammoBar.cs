using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ammoBar : MonoBehaviour
{
    public int maxAmmo = 20;
    public int currentAmmo;

    public Slider AmmoSlider;
    public Slider easeAmmoSlider;
    public float lerpSpeed = 0.03f;

    public Color lowAmmoColor = Color.white;
    public Color normalAmmoColor = new Color(32f / 255f, 178f / 255f, 170f / 255f, 1f);

    public Image ammoFill;
    public TMP_Text ammoText;

    void Start()
    {
        currentAmmo = maxAmmo;
        AmmoSlider.maxValue = maxAmmo;
        easeAmmoSlider.maxValue = maxAmmo;
        UpdateUI();
    }

    void Update()
    {
        if (AmmoSlider.value != currentAmmo)
            AmmoSlider.value = currentAmmo;

        if (easeAmmoSlider.value != currentAmmo)
            easeAmmoSlider.value = Mathf.Lerp(easeAmmoSlider.value, currentAmmo, lerpSpeed);
    }

    public void ReduceAmmo(int amount)
    {
        currentAmmo -= amount;
        if (currentAmmo < 0) currentAmmo = 0;
        UpdateUI();
    }

    public void Reload(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo) currentAmmo = maxAmmo;
        UpdateUI();
    }

    public void ReloadFull()
    {
        currentAmmo = maxAmmo;
        UpdateUI();
    }

    private void UpdateUI()
    {
        float ammoPercent = (float)currentAmmo / maxAmmo;
        ammoFill.color = ammoPercent < 0.3f ? lowAmmoColor : normalAmmoColor;

        if (ammoText != null)
            ammoText.text = currentAmmo + " / " + maxAmmo;
    }
}


