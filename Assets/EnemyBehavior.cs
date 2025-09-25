using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    private Animator animator;

    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 3f;
    public int damage = 10;
    public bool isBoss = false;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    [Header("Navigation Targets")]
    [Tooltip("Where the enemy marches when the player is not nearby.")]
    [SerializeField] private Transform strongholdTarget;
    [Tooltip("Scene tag used to auto-find the stronghold when no reference is assigned.")]
    [SerializeField] private string strongholdTag = "Base";
    [Tooltip("Optional anchor (e.g. invisible child) that represents the player's detection point.")]
    [SerializeField] private Transform playerDetectionTarget;
    [Tooltip("Minimum seconds between path recalculations to reduce jitter.")]
    [SerializeField] private float repathInterval = 0.25f;

    private int currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private float lastDestinationUpdateTime;
    private bool isDead = false;
    private string lootTableName;
    private float lootDropChance = 0f;
    private string bossLootTableName;

    // Events
    public System.Action<GameObject> OnDeath;

    public bool IsBoss => isBoss;
    public string LootTableName => lootTableName;
    public string BossLootTableName => string.IsNullOrEmpty(bossLootTableName) ? lootTableName : bossLootTableName;
    public float LootDropChance => Mathf.Clamp01(lootDropChance);

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent    = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
    }

    public void Initialize(EnemyType enemyType)
    {
        maxHealth       = enemyType.health;
        moveSpeed       = enemyType.speed;
        damage          = enemyType.damage;
        isBoss          = enemyType.isBoss;
        lootTableName   = enemyType.lootTableName;
        lootDropChance  = enemyType.lootDropChance;
        bossLootTableName = enemyType.bossLootTableName;

        currentHealth = maxHealth;

        TryCachePlayer();
        AutoAssignStronghold();

        agent.speed          = moveSpeed;
        agent.stoppingDistance = attackRange;

        AddVisualRepresentation();
    }

    void AddVisualRepresentation()
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = isBoss ? Vector3.one * 2f : Vector3.one;

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
            r.material.color = isBoss ? Color.red : Color.blue;

        Destroy(visual.GetComponent<Collider>()); // keep only main collider
    }

    void Update()
    {
        if (isDead) return;

        if (player == null) TryCachePlayer();

        Transform detectionTarget = GetDetectionTarget();
        float distanceToPlayer = detectionTarget != null
            ? Vector3.Distance(transform.position, detectionTarget.position)
            : float.MaxValue;

        Transform destination = strongholdTarget;
        if (detectionTarget != null && distanceToPlayer <= detectionRange)
            destination = detectionTarget;

        if (agent && destination && Time.time >= lastDestinationUpdateTime + repathInterval)
        {
            agent.SetDestination(destination.position);
            lastDestinationUpdateTime = Time.time;
        }

        if (player && distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            AttackPlayer();
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        Status playerStatus = player.GetComponent<Status>();
        if (playerStatus != null)
            playerStatus.TakeDamage(damage);

        Debug.Log($"{name} attacked player for {damage} damage!");
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        Debug.Log($"{name} get hit and take {dmg} damage! Current hp: {currentHealth}");

        if (currentHealth <= 0) Die();
    }

     


    void Die()
    {
        isDead = true;

        if (agent) agent.isStopped = true;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Reward player
        Leveling lvl = player ? player.GetComponent<Leveling>() : null;
        if (lvl != null)
            lvl.AddExperience(isBoss ? 100f : 20f);

        if (MetaProgression.Instance)
            MetaProgression.Instance.KillEnemy(name, isBoss);

        OnDeath?.Invoke(gameObject);

        //Death animation if available
        if (animator)
        {
            animator.SetTrigger("DEATH");
            StartCoroutine(DelayedDestroy());
        }
        else
        {
            Destroy(gameObject); // no animator → destroy immediately
        }
    }

    IEnumerator DelayedDestroy()
    {
        // Wait a frame to let animator switch states
        yield return null;
        float len = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, len + 0.2f);
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
            AttackPlayer();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ConfigureTargets(Transform stronghold, Transform detectionAnchor)
    {
        if (stronghold) strongholdTarget = stronghold;
        if (detectionAnchor) playerDetectionTarget = detectionAnchor;
    }

    void TryCachePlayer()
    {
        if (player) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        if (!playerDetectionTarget && player)
            playerDetectionTarget = player;
    }

    void AutoAssignStronghold()
    {
        if (strongholdTarget || string.IsNullOrEmpty(strongholdTag)) return;

        GameObject s = GameObject.FindGameObjectWithTag(strongholdTag);
        if (s) strongholdTarget = s.transform;
    }

    Transform GetDetectionTarget()
    {
        return playerDetectionTarget ? playerDetectionTarget : player;
    }
}
