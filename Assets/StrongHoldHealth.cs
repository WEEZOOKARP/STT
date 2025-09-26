
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class StrongholdHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDestroyed;

    [Header("Damage Handling")]
    [Tooltip("Used if EnemyBehavior doesn't expose a damage field.")]
    public int fallbackEnemyDamage = 10;

    void Awake()
    {
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
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

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
