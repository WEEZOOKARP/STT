
using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class StrongholdHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Damage Handling")]
    [Tooltip("Used if EnemyBehavior doesn't expose a damage field.")]
    public int fallbackEnemyDamage = 10;

    [Header("Zero-HP Behaviour")]
    [Tooltip("Destroy this stronghold object when health reaches zero.")]
    public bool destroyOnZeroHealth = true;
    [Tooltip("Optionally notify GameManager.GameOver() on destruction.")]
    public bool triggerGameOver = true;
    [Tooltip("Delay before destroying, to allow VFX/SFX.")]
    public float destroyDelay = 0.15f;

    // Reference to HUD bar (auto-assigned)
    private StrongholdHealthBar hudBar;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);

        // Auto-find HUD bar in scene
        hudBar = FindFirstObjectByType<StrongholdHealthBar>();
        if (hudBar == null)
        {
            Debug.LogWarning("No StrongholdHealthBar found in scene. HUD will not update.");
        }
        else
        {
            hudBar.SetHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        // Show damage number
        if (DamageNumberController.Instance != null)
        {
            // Stronghold damage is always "critical" since it's significant
            bool isCritical = amount >= 5; // Consider 5+ damage as critical for stronghold
            DamageNumberController.Instance.ShowDamageNumber(transform.position + Vector3.up * 2f, amount, false, isCritical, true);
        }

        if (DamageIndicatorController.Instance != null)
        {
            DamageIndicatorController.Instance.ReportDamage(transform.position, DamageIndicatorType.Stronghold);
        }

        // Update HUD
        if (hudBar != null)
            hudBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            Debug.Log("[Stronghold] Destroyed!");

            if (hudBar != null)
                hudBar.HideOnDestroyed();

            if (triggerGameOver && GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }

            if (destroyOnZeroHealth)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        if (hudBar != null)
            hudBar.SetHealth(currentHealth, maxHealth);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        int dmg = fallbackEnemyDamage;
        var eb = other.GetComponent<EnemyBehavior>();
        if (eb != null) { dmg = eb.damage; }

        TakeDamage(dmg);

        var enemyGo = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        Destroy(enemyGo);
    }
}
