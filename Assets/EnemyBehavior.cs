using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 3f;
    public int damage = 10;
    public bool isBoss = false;
    
    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    
    private int currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isDead = false;
    
    // Events
    public System.Action<GameObject> OnDeath;
    public Animator animator;
    
    public bool IsBoss => isBoss;
    
    public void Initialize(EnemyType enemyType)
    {
        maxHealth = enemyType.health;
        moveSpeed = enemyType.speed;
        damage = enemyType.damage;
        isBoss = enemyType.isBoss;
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Setup NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
        
        // Add visual representation
        AddVisualRepresentation();
    }
    
    void AddVisualRepresentation()
    {
        // Create a simple visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = isBoss ? Vector3.one * 2f : Vector3.one;
        
        // Set color based on enemy type
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (isBoss)
            {
                renderer.material.color = Color.red;
            }
            else
            {
                renderer.material.color = Color.blue;
            }
        }
        
        // Remove collider from visual (we'll use the main object's collider)
        Destroy(visual.GetComponent<Collider>());
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player
            agent.SetDestination(player.position);
            
            // Attack if in range
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }
    
    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        
        // Deal damage to player
        Status playerStatus = player.GetComponent<Status>();
        if (playerStatus != null)
        {
            playerStatus.TakeDamage(damage);
        }
        
        Debug.Log($"{gameObject.name} attacked player for {damage} damage!");
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator DamageFlash()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];
        
        // Store original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
            renderers[i].material.color = Color.white;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }
    
    void Die()
{
    isDead = true;
    animator.SetTrigger("DEATH");

    // Stop movement & collisions
    if (agent) agent.enabled = false;
    Collider col = GetComponent<Collider>();
    if (col) col.enabled = false;

    // Reward player
    Leveling playerLeveling = player?.GetComponent<Leveling>();
    if (playerLeveling != null)
    {
        float expReward = isBoss ? 100f : 20f;
        playerLeveling.AddExperience(expReward);
    }

    if (MetaProgression.Instance != null)
    {
        MetaProgression.Instance.KillEnemy(gameObject.name, isBoss);
    }

    OnDeath?.Invoke(gameObject);

    // ✅ Wait for animation length
    float deathTime = animator.GetCurrentAnimatorStateInfo(0).length;
    Destroy(gameObject, deathTime + 0.2f);
}

    
    void OnCollisionEnter(Collision collision)
    {
        // Handle collision with player
        if (collision.gameObject.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
