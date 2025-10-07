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
    public int hp;
    public int maxHP;
    private bool inDanger = false;
    public ParticleSystem hitSparkEffect;
    public ParticleSystem ShitSparkEffect;

    void Start()
    {
        maxHP = (int)slider.maxValue;
        hp = maxHP;
        UpdateBar();
    }

    void Update()
    {
        slider.value = hp;
        easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, hp, lerpSpeed * Time.deltaTime * 60f);

        // Test inputs
        if (Input.GetKeyDown(KeyCode.Space)) TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.H)) Heal(20);
    }

    public void SetMaxHealth(int health)
    {
        maxHP = health;
        slider.maxValue = maxHP;
        hp = maxHP;
        UpdateBar();
    }

    public void SetHealth(int health)
    {
        hp = Mathf.Clamp(health, 0, maxHP);
        UpdateBar();
    }

    public void TakeDamage(int damage){
        SetHealth(hp - damage);
        if (hitSparkEffect != null && slider.value != 0)
        {
            hitSparkEffect.Play();
            if (ShitSparkEffect != null)
                ShitSparkEffect.Play();
        }
    }
    public void Heal(int recover) => SetHealth(hp + recover);

    void UpdateBar()
    {
        float hpPercent = (float)hp / maxHP;

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

    public bool isInDanger() => inDanger;
    public float getHealthPercent() => (float)hp / maxHP;
}

