using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{

    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 3f;
    public int damage = 10;
    public bool isBoss = false;

    private float baseMoveSpeed;
    private float speedMultiplier = 1f;
    private float damageTakenMultiplier = 1f;

    // For tutorial hints.
    public static event System.Action<string, bool> OnEnemySpawned;

    [Header("AI Settings")]
    [Tooltip("How often we can recompute a path (sec).")]
    [SerializeField] private float repathInterval = 0.25f;
    [Tooltip("Distance to the player to START chasing.")]
    [SerializeField] private float chaseRange = 10f;
    [Tooltip("Distance to the player to STOP chasing (slightly larger than chaseRange to avoid flip-flop).")]
    [SerializeField] private float disengageRange = 13f;
    [Tooltip("Within this distance of the stronghold, count as a hit even if we stop a few cm short.")]
    [SerializeField] private float baseContactDistance = 0.75f;
    [Tooltip("NavMesh sample radius used for SetDestination safety.")]
    [SerializeField] private float navSampleRadius = 1.0f;
    [SerializeField] private Transform targetAnchor;


    [Header("Navigation Targets")]
    [Tooltip("Where the enemy marches when not pursuing the player.")]
    [SerializeField] private Transform strongholdTarget;
    [Tooltip("Scene tag used to auto-find the stronghold when no reference is assigned.")]
    [SerializeField] private string strongholdTag = "Base";
    [Tooltip("Optional anchor (e.g. invisible child) representing the player's detection point.")]
    [SerializeField] private Transform playerDetectionTarget;

    // this is will be used for gold when enemies die
    [Header("Rewards")]
    public int goldReward = 1;
    public int bossGoldReward = 5;

    [Header("Coin Drop (visual only)")]
    public GameObject coinPrefab;
  



    private int currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private float lastDestinationUpdateTime;
    private bool isDead = false;

    private string lootTableName;
    private float lootDropChance = 0f;
    private string bossLootTableName;

    public System.Action<GameObject> OnDeath;

    public bool IsBoss => isBoss;
    public string LootTableName => lootTableName;
    public string BossLootTableName => string.IsNullOrEmpty(bossLootTableName) ? lootTableName : bossLootTableName;
    public float LootDropChance => Mathf.Clamp01(lootDropChance);

    enum TargetMode { Stronghold, ChasePlayer }
    private TargetMode mode = TargetMode.Stronghold;

    // ----------------- Initialization -----------------
    public void Initialize(EnemyType enemyType)
    {
        maxHealth = enemyType.health;
        moveSpeed = enemyType.speed * PlayerPrefs.GetFloat("enemyDMG", 1f);
        damage = (int)Math.Floor(enemyType.damage * PlayerPrefs.GetFloat("enemyDMG", 1f)); ;
        isBoss = enemyType.isBoss;
        lootTableName = enemyType.lootTableName;
        lootDropChance = enemyType.lootDropChance;
        bossLootTableName = enemyType.bossLootTableName;

        currentHealth = maxHealth;

        TryCachePlayer();
        AutoAssignStronghold();

        agent = GetComponent<NavMeshAgent>();
        if (!agent) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.15f; // lets them tuck in close
        agent.autoBraking = true;

        // Make sure we start on the NavMesh if spawned slightly off
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        AddVisualRepresentation();
    }

    void Awake()
    {
        if (targetAnchor == null)
        {
            var go = GameObject.FindWithTag("PlayerAnchor");
            if (go) targetAnchor = go.transform;
            else Debug.LogWarning("EnemyBehavior: No object tagged 'PlayerAnchor' found.");
        }
    }

    void Start()
    {
        // Fire event for tutorial hint system to detect enemy spawns
        if (OnEnemySpawned != null)
        {
            OnEnemySpawned.Invoke(gameObject.name, isBoss);
            Debug.Log($"[EnemyBehavior] OnEnemySpawned event fired for {gameObject.name} (boss: {isBoss})");
        }
    }

    void AddVisualRepresentation()
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = isBoss ? Vector3.one * 2f : Vector3.one;
        var renderer = visual.GetComponent<Renderer>();
        if (renderer) renderer.material.color = isBoss ? Color.red : Color.blue;
        Destroy(visual.GetComponent<Collider>());
    }

    // ----------------- Frame Update -----------------
    void Update()
    {
        if (isDead) return;

        if (!player) TryCachePlayer();
        Transform detectionTarget = GetDetectionTarget();
        float distToPlayer = detectionTarget ? Vector3.Distance(transform.position, detectionTarget.position) : float.MaxValue;
        float distToBase = strongholdTarget ? Vector3.Distance(transform.position, strongholdTarget.position) : float.MaxValue;

        // Decide mode with hysteresis to avoid jitter
        switch (mode)
        {
            case TargetMode.Stronghold:
                if (detectionTarget && distToPlayer <= chaseRange)
                    mode = TargetMode.ChasePlayer;
                break;
            case TargetMode.ChasePlayer:
                if (!detectionTarget || distToPlayer >= disengageRange)
                    mode = TargetMode.Stronghold;
                break;
        }

        // Choose destination
        Transform desired = (mode == TargetMode.ChasePlayer) ? detectionTarget : strongholdTarget;

        if (AgentReady() && desired && Time.time >= lastDestinationUpdateTime + repathInterval)
        {
            // Sample on NavMesh for safe SetDestination
            Vector3 dest = desired.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(dest, out hit, navSampleRadius, NavMesh.AllAreas))
                SetSafeDestination(hit.position);
            else
                SetSafeDestination(dest); // fallback
            lastDestinationUpdateTime = Time.time;
        }

        // Base contact fallback (scores a hit even if the agent stops a hair short)
        if (strongholdTarget && distToBase <= baseContactDistance)
        {
            DamageStronghold(1); // your forwarder to the stronghold’s TakeDamage
            Die();               // kamikaze behaviour when they reach the base
            return;
        }

        // Player contact / melee
        if (player && distToPlayer <= 1.75f && Time.time >= lastAttackTime + 1.0f)
        {
            AttackPlayer();
        }
    }

    // ----------------- Actions -----------------
    void AttackPlayer()
    {
        lastAttackTime = Time.time;
        Status playerStatus = player ? player.GetComponent<Status>() : null;
        if (playerStatus) playerStatus.TakeDamage(damage);
    }

public void TakeDamage(int amount)
{
    if (isDead) return;
    var lvl = player ? player.GetComponent<Leveling>() : null;

    float playerDamageMultiplier = 1f;
    if (lvl != null && lvl.playerStats != null)
    {
        playerDamageMultiplier = lvl.playerStats.GetPlayerMultipliers().Damage;
    }

    int finalDamage = Mathf.RoundToInt(amount * damageTakenMultiplier * playerDamageMultiplier);
    // Report hit to DailyTaskManager
    DailyTaskManager.Instance?.OnEnemyTakeDamage();

    currentHealth -= finalDamage;
    StartCoroutine(DamageFlash());

    if (currentHealth <= 0)
        Die();
}


    System.Collections.IEnumerator DamageFlash()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var originals = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originals[i] = renderers[i].material.color;
            renderers[i].material.color = Color.white;
        }
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originals[i];
    }

    void Die()
{
    if (isDead) return;
    isDead = true;

    if (agent) agent.enabled = false;
    var col = GetComponent<Collider>();
    if (col) col.enabled = false;

    try
    {
        // Award XP
        Leveling lvl = null;
        if (targetAnchor) lvl = targetAnchor.GetComponentInParent<Leveling>();
        if (lvl != null) lvl.AddExperience(isBoss ? 100f : 5f);

        // Meta kill stats
        var meta = MetaProgression.Instance;
        if (meta != null) meta.KillEnemy(gameObject.name, isBoss);

        // Report kill to DailyTaskManager
        DailyTaskManager.Instance?.OnEnemyKilled();

        // Visual coin drop (optional cosmetic)
        if (coinPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.25f;
            var go = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            var vis = go.GetComponent<CoinPickupVisual>();
            if (vis != null) vis.Initialize(targetAnchor);
        }

        // Gold payout
        if (GoldService.Instance != null)
            GoldService.Instance.Add(isBoss ? bossGoldReward : goldReward);
    }
    finally
    {
        OnDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }
}



    // ----------------- Helpers -----------------
    void SetSafeDestination(Vector3 worldPos)
    {
        if (!AgentReady()) return;
        // Only reset if significantly different to reduce path spam
        if (agent.destination == Vector3.zero || (agent.destination - worldPos).sqrMagnitude > 0.01f)
            agent.SetDestination(worldPos);
    }

    bool AgentReady()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    public void ConfigureTargets(Transform stronghold, Transform detectionAnchor)
    {
        strongholdTarget = stronghold ? stronghold : strongholdTarget;
        if (detectionAnchor) playerDetectionTarget = detectionAnchor;
    }

    void TryCachePlayer()
    {
        if (player) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        if (!playerDetectionTarget && player) playerDetectionTarget = player;
    }

    void AutoAssignStronghold()
    {
        if (strongholdTarget || string.IsNullOrEmpty(strongholdTag)) return;
        GameObject strongholdObj = GameObject.FindGameObjectWithTag(strongholdTag);
        if (strongholdObj) strongholdTarget = strongholdObj.transform;
    }

    Transform GetDetectionTarget()
    {
        if (playerDetectionTarget) return playerDetectionTarget;
        if (player) return player;
        return null;
    }

    // -------- Stronghold damage passthrough (keeps your single source of truth) --------
    public void DamageStronghold(int amount)
    {
        // This assumes you already have the forwarder that calls stronghold.TakeDamage(...)
        // If your Base has StrongholdHealth on the same transform, you can do:
        if (!strongholdTarget) return;
        var sh = strongholdTarget.GetComponent<StrongholdHealth>();
        if (sh) sh.TakeDamage(amount);
    }

    // ----------------- Gizmos -----------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.75f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, disengageRange);
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        if (strongholdTarget) Gizmos.DrawWireSphere(strongholdTarget.position, baseContactDistance);
    }

    // Keep simple collision hook if you want contact damage too
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player")) AttackPlayer();
    }
    public void ApplySpeedMultiplier(float mult)
    {
        speedMultiplier *= mult;
        if (agent != null) agent.speed = baseMoveSpeed * speedMultiplier;
    }

    public void ResetSpeedMultiplier()
    {
        speedMultiplier = 1f;
        if (agent != null) agent.speed = baseMoveSpeed;
    }

    public void ApplyDamageMultiplier(float mult)
    {
        damageTakenMultiplier *= mult;
    }

    public void ResetDamageMultiplier()
    {
        damageTakenMultiplier = 1f;
    }
}
