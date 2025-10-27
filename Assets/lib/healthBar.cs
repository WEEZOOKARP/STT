using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public Slider slider;
    public Slider easeHealthSlider;
    public float lerpSpeed = 0.03f;
    public Image fill;
    public Color highHPColor = Color.green;
    public Color midHPColor = Color.yellow;
    public Color lowHPColor = Color.red;

    private int hp;
    private int maxHP;
    private bool inDanger = false;

    void Start()
    {
        // Initialize with default values if not set externally
        if (maxHP == 0)
        {
            maxHP = 100;
            hp = maxHP;
        }
    }

    void Update()
    {
        // Smooth lerp for the ease health slider
        if (easeHealthSlider != null)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, slider.value, lerpSpeed * Time.deltaTime * 60f);
        }
    }

    public void SetMaxHealth(int health)
    {
        maxHP = health;
        slider.maxValue = maxHP;
        if (easeHealthSlider != null)
        {
            easeHealthSlider.maxValue = maxHP;
        }
        hp = maxHP;
        slider.value = hp;
        if (easeHealthSlider != null)
        {
            easeHealthSlider.value = hp;
        }
        UpdateBar();
    }

    public void SetHealth(int health)
    {
        hp = Mathf.Clamp(health, 0, maxHP);
        slider.value = hp;
        UpdateBar();
    }

    void UpdateBar()
    {
        if (maxHP == 0) return;

        float hpPercent = (float)hp / maxHP;

        if (fill != null)
        {
            if (hpPercent > 0.5f)
            {
                fill.color = highHPColor;
                inDanger = false;
            }
            else if (hpPercent > 0.2f)
            {
                fill.color = midHPColor;
                inDanger = false;
            }
            else
            {
                fill.color = lowHPColor;
                inDanger = true;
            }
        }
    }

    public bool isInDanger() => inDanger;
    public float getHealthPercent() => maxHP > 0 ? (float)hp / maxHP : 0f;
    public int GetCurrentHealth() => hp;
    public int GetMaxHealth() => maxHP;
}