
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class StrongholdHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDestroyed;

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

    void Reset()
    {
        // Make sure our collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        // Start healthy and broadcast initial value
        currentHealth = Mathf.Max(1, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            Debug.Log("[Stronghold] Destroyed!");
            OnDestroyed?.Invoke();

            if (triggerGameOver && GameManager.Instance != null)
            {
                GameManager.Instance.GameOver(); // optional, remove if not desired
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
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Enemy hits the stronghold -> base takes damage, enemy dies.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        int dmg = fallbackEnemyDamage;
        var eb = other.GetComponent<EnemyBehavior>();
        if (eb != null) { dmg = eb.damage; }

        TakeDamage(dmg);

        // kill the enemy on contact
        var enemyGo = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        Destroy(enemyGo);
    }
}
