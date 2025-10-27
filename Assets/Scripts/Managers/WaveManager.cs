using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Data Classes
[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;
    public int health;
    public float speed;
    public int damage;
    public float spawnWeight = 1f;
    public bool isBoss = false;
    public string lootTableName;
    public float lootDropChance;
    public string bossLootTableName;
}

[System.Serializable]
public class WaveComposition
{
    public int waveNumber;
    public List<EnemySpawn> enemies;
    public bool isBossWave = false;
    public float waveDuration = 60f;
}

[System.Serializable]
public class EnemySpawn
{
    public string enemyTypeName;
    public int count;
    public float spawnDelay;
    public Vector3 spawnPosition;
}

[System.Serializable]
public class SpecialWaveModifier
{
    public enum ModifierType { DoubleEnemies, TougherEnemies }

    public ModifierType type;
    public string displayName;
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float lootBonusChance = 0f;
    public int countMultiplier = 1;
}
#endregion

public class WaveManager : MonoBehaviour
{
    #region Constants
    private const string ENEMY_TAG = "Enemy";
    private const string PLAYER_TAG = "Player";
    private const string DEFAULT_ENEMY_NAME = "Enemy";
    private const string DEFAULT_LOOT_TABLE = "BasicEnemy";
    private const string DEFAULT_BOSS_LOOT_TABLE = "BasicBoss";
    private const int MAX_WAVE_REROLL_ATTEMPTS = 5;
    private const int SAFETY_LOOP_LIMIT = 500;
    private const float PRUNE_INTERVAL = 2f;
    #endregion

    #region Serialized Fields
    [Header("References")]
    [SerializeField] private StrongholdHealth stronghold;

    [Header("Enemy Types")]
    public List<EnemyType> availableEnemyTypes = new List<EnemyType>();

    [Header("Faction Data")]
    public FactionEnemyPool activeFaction;
    public int bossWaveInterval = 5;

    [Header("Wave Settings")]
    public List<WaveComposition> predefinedWaves = new List<WaveComposition>();
    public int maxWaves = 10;
    public float timeBetweenWaves = 5f;
    public int baseWaveBudget = 6;
    public int budgetPerWave = 2;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnRadius = 10f;
    public float spawnDelayMin = 0.5f;
    public float spawnDelayMax = 2.5f;
    public float perEnemySpawnDelayMin = 0.1f;
    public float perEnemySpawnDelayMax = 0.35f;

    [Header("Special Wave Settings")]
    [SerializeField] private float specialWaveChance = 0.15f;
    [SerializeField] private float doubleEnemyChance = 0.5f;
    [SerializeField] private float toughEnemyHealthMultiplier = 2f;
    [SerializeField] private float toughEnemyDamageMultiplier = 1.25f;
    [SerializeField] private float toughEnemySpeedMultiplier = 1.25f;
    [SerializeField] private float toughEnemyLootBonus = 0.25f;

    [Header("Targets")]
    [Tooltip("Stronghold or base the enemies march toward when not pursuing the player.")]
    public Transform strongholdTarget;
    [Tooltip("Optional detection anchor (e.g. invisible child object) attached to the player.")]
    public Transform playerDetectionAnchor;

    [Header("Build Phases")]
    public bool useBuildPhases = true;

    [Header("State")]
    public int currentWave = 1;
    public bool isWaveActive = false;
    public int enemiesRemaining = 0;
    public bool buildPhaseActive = false;

    [Header("Debug")]
    [SerializeField] private bool autoStartOnAwake = false;
    [SerializeField] private bool logWaveCompositions = false;
    #endregion

    #region Private Fields
    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private readonly Dictionary<string, FactionEnemyEntry> entryLookup = new Dictionary<string, FactionEnemyEntry>();
    private readonly Dictionary<string, (int health, int damage, float speed, float lootDropChance)> originalStats = new Dictionary<string, (int, int, float, float)>();
    private readonly Dictionary<string, int> originalSpawnCounts = new Dictionary<string, int>();
    private readonly HashSet<string> generatedWaveSignatures = new HashSet<string>();

    private Coroutine currentWaveCoroutine;
    private System.Random fallbackRandom = new System.Random();
    private string lastWaveSignature;
    private float nextPruneTime = 0f;
    private SpecialWaveModifier currentWaveModifier;

    // Cached references
    private Transform cachedPlayerAnchor;
    private bool hasSearchedForPlayer = false;
    #endregion

    #region Properties
    public int StrongholdCurrentHealth => stronghold ? stronghold.CurrentHealth : 0;
    public int StrongholdMaxHealth => stronghold ? stronghold.MaxHealth : 0;
    #endregion

    #region Events
    public static event System.Action<int> OnWaveCompleted;
    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveComplete;
    public System.Action OnAllWavesComplete;
    public System.Action<int> OnBuildPhaseStarted;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        InitializeEnemyTypes();
    }

    void Start()
    {
        if (autoStartOnAwake)
        {
            BeginRun(currentWave);
        }
    }

    void Update()
    {
        if (isWaveActive && Time.time >= nextPruneTime)
        {
            SafetyPruneDeadEnemies();
            nextPruneTime = Time.time + PRUNE_INTERVAL;
        }
    }

    void OnValidate()
    {
        // Ensure valid values
        maxWaves = Mathf.Max(1, maxWaves);
        baseWaveBudget = Mathf.Max(1, baseWaveBudget);
        budgetPerWave = Mathf.Max(0, budgetPerWave);
        timeBetweenWaves = Mathf.Max(0f, timeBetweenWaves);
        spawnRadius = Mathf.Max(0f, spawnRadius);
        specialWaveChance = Mathf.Clamp01(specialWaveChance);
        doubleEnemyChance = Mathf.Clamp01(doubleEnemyChance);
    }
    #endregion

    #region Initialization
    /// Initializes enemy types from the active faction or creates default enemies.
    void InitializeEnemyTypes()
    {
        if (availableEnemyTypes == null)
        {
            availableEnemyTypes = new List<EnemyType>();
        }

        availableEnemyTypes.Clear();
        entryLookup.Clear();

        if (activeFaction != null && activeFaction.enemies != null)
        {
            foreach (var entry in activeFaction.enemies)
            {
                if (entry == null) continue;
                RegisterEnemyEntry(entry);
            }
        }

        if (availableEnemyTypes.Count == 0)
        {
            CreateDefaultEnemies();
        }
    }

    /// Creates default enemy types when no faction is configured.
    void CreateDefaultEnemies()
    {
        RegisterEnemyEntry(new FactionEnemyEntry
        {
            displayName = "Basic Enemy",
            health = 50,
            speed = 3f,
            damage = 10,
            spawnWeight = 1f,
            difficultyCost = 1,
            lootTableName = DEFAULT_LOOT_TABLE,
            lootDropChance = 0.25f
        });

        RegisterEnemyEntry(new FactionEnemyEntry
        {
            displayName = "Fast Enemy",
            health = 30,
            speed = 5f,
            damage = 5,
            spawnWeight = 0.7f,
            difficultyCost = 1,
            lootTableName = "FastEnemy",
            lootDropChance = 0.35f
        });

        RegisterEnemyEntry(new FactionEnemyEntry
        {
            displayName = "Tank Enemy",
            health = 100,
            speed = 2f,
            damage = 15,
            spawnWeight = 0.5f,
            difficultyCost = 2,
            lootTableName = "TankEnemy",
            lootDropChance = 0.4f
        });

        RegisterEnemyEntry(new FactionEnemyEntry
        {
            displayName = "Boss",
            health = 200,
            speed = 2.5f,
            damage = 25,
            spawnWeight = 0.1f,
            isBoss = true,
            difficultyCost = 10,
            minWave = bossWaveInterval,
            maxPerWave = 1,
            lootTableName = DEFAULT_BOSS_LOOT_TABLE,
            lootDropChance = 1f,
            bossLootTableName = DEFAULT_BOSS_LOOT_TABLE
        });
    }

    /// Registers a faction enemy entry into the available enemy types.
    void RegisterEnemyEntry(FactionEnemyEntry entry)
    {
        if (entry == null) return;

        EnemyType type = entry.ToEnemyType();
        string key = ResolveEntryName(entry);

        entryLookup[key] = entry;
        availableEnemyTypes.Add(type);
    }
    #endregion

    #region Wave Control
    /// Begins a new run starting from the specified wave.
    public void BeginRun(int startingWave = 1)
    {
        InitializeEnemyTypes();
        ResetState();
        currentWave = Mathf.Max(1, startingWave);

        if (stronghold)
        {
            stronghold.ResetHealth();
        }

        StartNextWave();
    }

    /// Stops the current wave and resets all state.
    public void StopCurrentWave()
    {
        ResetState();
    }

    /// Resets the wave manager state.
    void ResetState()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        activeEnemies.Clear();
        enemiesRemaining = 0;
        isWaveActive = false;
        buildPhaseActive = false;
        generatedWaveSignatures.Clear();
        lastWaveSignature = null;
        currentWaveModifier = null;
    }
    #endregion

    #region Wave Execution
    /// Executes a single wave from start to completion.
    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] StartNextWave() -> wave {currentWave}");
        if (currentWave > maxWaves) { OnAllWavesComplete?.Invoke(); return; }
        buildPhaseActive = false;
        musicManager mm = FindObjectOfType<musicManager>();
        mm.beginPlay("Battle");
        currentWaveCoroutine = StartCoroutine(RunWave(currentWave));
    }

    IEnumerator RunWave(int waveNumber)
    {
        Debug.Log($"[WaveManager] Running wave {waveNumber}");
        isWaveActive = true;
        OnWaveStart?.Invoke(waveNumber);

        System.Random waveRandom = GetWaveRandom(waveNumber);

        // Generate wave composition
        WaveComposition waveComp = GenerateRandomWave(waveNumber, waveRandom);

        // Apply special wave modifier
        currentWaveModifier = null;
        if (UnityEngine.Random.value < specialWaveChance)
        {
            currentWaveModifier = CreateSpecialWaveModifier(waveComp, waveNumber);
        }

        // Spawn enemies
        yield return StartCoroutine(SpawnWaveEnemies(waveComp, waveRandom));

        // Wait for all enemies to be defeated
        while (true)
        {
            SafetyPruneDeadEnemies();

            if (enemiesRemaining <= 0 || activeEnemies.Count == 0)
            {
                break;
            }

            yield return null;
        }

        // Wave complete
        CleanupWave(waveComp);
        isWaveActive = false;
        OnWaveComplete?.Invoke(waveNumber);
        OnWaveCompleted?.Invoke(waveNumber);

        // Track wave completion
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.CompleteWave(waveNumber);
        }

        StartBuildPhase();
    }

    /// Cleans up wave-specific data and modifiers.
    void CleanupWave(WaveComposition wave)
    {
        RestoreOriginalEnemyStats();
        originalSpawnCounts.Clear();
        currentWaveModifier = null;
        musicManager mm = FindObjectOfType<musicManager>();
        buildPhaseActive = true;
        isWaveActive = false;
        mm.beginPlay("Calm");
        // fire the event (keeps things decoupled if you use it)
        OnBuildPhaseStarted?.Invoke(currentWave);

        Debug.Log($"[WaveManager] Wave {wave.waveNumber} cleanup complete.");
    }
    #endregion

    #region Wave Generation

    /// Generates a randomized wave composition based on budget and available enemies.
    WaveComposition GenerateRandomWave(int waveNumber, System.Random rng, int rerollDepth = 0)
    {
        WaveComposition wave = new WaveComposition
        {
            waveNumber = waveNumber,
            enemies = new List<EnemySpawn>()
        };

        List<FactionEnemyEntry> eligibleEntries = GatherEligibleEntries(waveNumber);
        if (eligibleEntries.Count == 0)
        {
            Debug.LogWarning($"WaveManager: No eligible enemies found for wave {waveNumber}");
            return wave;
        }

        bool isBossWave = bossWaveInterval > 0 && waveNumber % bossWaveInterval == 0 && eligibleEntries.Any(e => e.isBoss);
        wave.isBossWave = isBossWave;

        Dictionary<FactionEnemyEntry, int> counts = new Dictionary<FactionEnemyEntry, int>();
        int targetBudget = CalculateWaveBudget(waveNumber, rng, isBossWave);
        int usedBudget = 0;

        // Spawn boss first if boss wave
        if (isBossWave)
        {
            var bossEntry = PickEnemyEntry(eligibleEntries, rng, counts, int.MaxValue, true);
            if (bossEntry != null)
            {
                RegisterSpawn(bossEntry, counts);
                usedBudget += Mathf.Max(1, bossEntry.difficultyCost);
            }
        }

        // Fill remaining budget with regular enemies
        int safety = 0;
        while (usedBudget < targetBudget && safety < SAFETY_LOOP_LIMIT)
        {
            safety++;
            int remainingBudget = Math.Max(1, targetBudget - usedBudget);
            var entry = PickEnemyEntry(eligibleEntries, rng, counts, remainingBudget, false);

            if (entry == null)
            {
                break;
            }

            int cost = Mathf.Max(1, entry.difficultyCost);
            if (usedBudget + cost > targetBudget && counts.Count > 0)
            {
                break;
            }

            RegisterSpawn(entry, counts);
            usedBudget += cost;
        }

        wave.enemies = BuildEnemySpawns(counts, rng);

        if (logWaveCompositions)
        {
            LogWaveComposition(waveNumber, wave, targetBudget, usedBudget);
        }

        // Check for duplicate waves and reroll if needed
        string signature = BuildWaveSignature(wave);
        if (IsDuplicateSignature(signature) && rerollDepth < MAX_WAVE_REROLL_ATTEMPTS)
        {
            if (logWaveCompositions)
            {
                Debug.Log($"WaveManager: Rerolling wave {waveNumber} (duplicate signature)");
            }

            return GenerateRandomWave(waveNumber, rng, rerollDepth + 1);
        }

        lastWaveSignature = signature;
        generatedWaveSignatures.Add(signature);
        return wave;
    }


    /// Calculates the budget for a wave based on wave number and variance.
    int CalculateWaveBudget(int waveNumber, System.Random rng, bool isBossWave)
    {
        int baseBudgetValue = Mathf.Max(1, baseWaveBudget + (waveNumber - 1) * budgetPerWave);
        int variance = Mathf.Max(1, Mathf.RoundToInt(baseBudgetValue * 0.25f));
        int minBudget = Mathf.Max(1, baseBudgetValue - variance);
        int maxBudget = Mathf.Max(minBudget + 1, baseBudgetValue + variance);
        int budget = minBudget + rng.Next(maxBudget - minBudget + 1);

        if (isBossWave)
        {
            budget = Mathf.Max(budget, minBudget + budgetPerWave * 2);
        }

        return budget;
    }

    /// Gathers all enemies eligible for spawning in the given wave.
    List<FactionEnemyEntry> GatherEligibleEntries(int waveNumber)
    {
        List<FactionEnemyEntry> entries = new List<FactionEnemyEntry>();

        foreach (var pair in entryLookup)
        {
            FactionEnemyEntry entry = pair.Value;
            if (entry != null && entry.IsAvailableForWave(waveNumber))
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    /// Picks a random enemy from the eligible list based on spawn weights.
    FactionEnemyEntry PickEnemyEntry(List<FactionEnemyEntry> entries, System.Random rng, Dictionary<FactionEnemyEntry, int> counts, int remainingBudget, bool bossOnly)
    {
        List<FactionEnemyEntry> candidates = new List<FactionEnemyEntry>();

        foreach (var entry in entries)
        {
            if (entry == null) continue;
            if (bossOnly && !entry.isBoss) continue;
            if (!bossOnly && entry.isBoss) continue;

            if (entry.maxPerWave > 0 && counts.TryGetValue(entry, out int currentCount) && currentCount >= entry.maxPerWave)
            {
                continue;
            }

            int cost = Mathf.Max(1, entry.difficultyCost);
            if (cost > remainingBudget && remainingBudget > 0)
            {
                continue;
            }

            candidates.Add(entry);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        float totalWeight = candidates.Sum(candidate => Mathf.Max(0.01f, candidate.spawnWeight));
        double roll = rng.NextDouble() * totalWeight;

        foreach (var candidate in candidates)
        {
            float weight = Mathf.Max(0.01f, candidate.spawnWeight);
            roll -= weight;
            if (roll <= 0)
            {
                return candidate;
            }
        }

        return candidates[candidates.Count - 1];
    }

    /// Builds the list of enemy spawns from the count dictionary.
    List<EnemySpawn> BuildEnemySpawns(Dictionary<FactionEnemyEntry, int> counts, System.Random rng)
    {
        List<EnemySpawn> spawns = new List<EnemySpawn>();

        foreach (var pair in counts)
        {
            EnemyType type = GetEnemyTypeForEntry(pair.Key);
            if (type == null) continue;

            spawns.Add(new EnemySpawn
            {
                enemyTypeName = type.name,
                count = pair.Value,
                spawnDelay = Mathf.Lerp(spawnDelayMin, spawnDelayMax, (float)rng.NextDouble()),
                spawnPosition = Vector3.zero
            });
        }

        Shuffle(spawns, rng);
        return spawns;
    }

    /// Registers a spawn in the count dictionary.
    void RegisterSpawn(FactionEnemyEntry entry, Dictionary<FactionEnemyEntry, int> counts)
    {
        if (counts.TryGetValue(entry, out int current))
        {
            counts[entry] = current + 1;
        }
        else
        {
            counts[entry] = 1;
        }
    }
    #endregion

    #region Enemy Spawning
    /// Spawns all enemies in the wave composition with delays.
    IEnumerator SpawnWaveEnemies(WaveComposition wave, System.Random rng)
    {
        enemiesRemaining = wave.enemies.Sum(e => e.count);

        foreach (EnemySpawn spawn in wave.enemies)
        {
            float delay = Mathf.Clamp(spawn.spawnDelay, 0f, Mathf.Max(spawnDelayMax, spawnDelayMin));
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            for (int i = 0; i < spawn.count; i++)
            {
                SpawnEnemy(spawn.enemyTypeName, rng);

                float perSpawnDelay = Mathf.Lerp(perEnemySpawnDelayMin, perEnemySpawnDelayMax, (float)rng.NextDouble());
                if (perSpawnDelay > 0f)
                {
                    yield return new WaitForSeconds(perSpawnDelay);
                }
            }
        }
    }

    /// Spawns a single enemy at a random spawn point.
    void SpawnEnemy(string enemyTypeName, System.Random rng)
    {
        if (string.IsNullOrEmpty(enemyTypeName))
        {
            Debug.LogError("WaveManager: Cannot spawn enemy with null/empty type name");
            return;
        }

        if (rng == null)
        {
            Debug.LogError("WaveManager: RNG cannot be null for spawning");
            return;
        }

        EnemyType enemyType = availableEnemyTypes.Find(e => e.name == enemyTypeName);
        if (enemyType == null)
        {
            Debug.LogWarning($"WaveManager: Enemy type not found for name {enemyTypeName}");
            return;
        }

        Vector3 spawnPos = GetRandomSpawnPosition(rng);
        GameObject enemy;

        if (enemyType.prefab != null)
        {
            enemy = Instantiate(enemyType.prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            enemy = new GameObject(enemyType.name);
            enemy.transform.position = spawnPos;
        }

        enemy.name = enemyType.name;
        enemy.tag = ENEMY_TAG;

        EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
        if (behavior == null)
        {
            behavior = enemy.AddComponent<EnemyBehavior>();
        }

        behavior.Initialize(enemyType);

        // Apply special wave modifiers if active
        if (currentWaveModifier != null)
        {
            ApplyModifierToEnemy(behavior, currentWaveModifier);
        }

        behavior.ConfigureTargets(strongholdTarget, ResolvePlayerAnchor());
        behavior.OnDeath -= OnEnemyDeath;
        behavior.OnDeath += OnEnemyDeath;

        activeEnemies.Add(enemy);
    }

    /// Gets a random spawn position from configured spawn points.
    Vector3 GetRandomSpawnPosition(System.Random rng)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = rng.Next(spawnPoints.Length);
            Transform spawnPoint = spawnPoints[index];
            Vector3 offset = RandomPointInCircle(rng, spawnRadius);
            offset.y = 0f;
            return spawnPoint.position + offset;
        }

        return RandomPointInCircle(rng, spawnRadius);
    }

    /// Generates a random point within a circle.
    Vector3 RandomPointInCircle(System.Random rng, float radius)
    {
        if (radius <= 0f)
        {
            return Vector3.zero;
        }

        double angle = rng.NextDouble() * Math.PI * 2.0;
        double distance = Math.Sqrt(rng.NextDouble()) * radius;
        return new Vector3((float)(Math.Cos(angle) * distance), 0f, (float)(Math.Sin(angle) * distance));
    }
    #endregion

    #region Special Wave Modifiers
    /// Creates and applies a special wave modifier.
    SpecialWaveModifier CreateSpecialWaveModifier(WaveComposition wave, int waveNumber)
    {
        if (wave == null || wave.enemies == null || wave.enemies.Count == 0)
        {
            return null;
        }

        bool isDoubleWave = UnityEngine.Random.value < doubleEnemyChance;

        SpecialWaveModifier modifier = new SpecialWaveModifier();

        if (isDoubleWave)
        {
            modifier.type = SpecialWaveModifier.ModifierType.DoubleEnemies;
            modifier.displayName = "DOUBLE TROUBLE";
            modifier.countMultiplier = 2;

            // Double enemy counts for spawn
            foreach (var spawn in wave.enemies)
            {
                string key = $"{spawn.enemyTypeName}_{waveNumber}";
                if (!originalSpawnCounts.ContainsKey(key))
                {
                    originalSpawnCounts[key] = spawn.count;
                }
                spawn.count *= 2;
            }
        }
        else
        {
            modifier.type = SpecialWaveModifier.ModifierType.TougherEnemies;
            modifier.displayName = "TOUGHER ENEMIES";
            modifier.healthMultiplier = toughEnemyHealthMultiplier;
            modifier.damageMultiplier = toughEnemyDamageMultiplier;
            modifier.speedMultiplier = toughEnemySpeedMultiplier;
            modifier.lootBonusChance = toughEnemyLootBonus;
        }

        warningCuscene.Instance?.showWarningCuscene();

        Debug.Log($" [Special Wave] Wave {waveNumber} is {modifier.displayName}!");
        return modifier;
    }

    /// <summary>
    /// Applies special wave modifier to an individual enemy.
    /// </summary>
    void ApplyModifierToEnemy(EnemyBehavior behavior, SpecialWaveModifier modifier)
    {
        if (behavior == null || modifier == null) return;

        if (modifier.type == SpecialWaveModifier.ModifierType.TougherEnemies)
        {
            behavior.ApplyWaveModifiers(
                modifier.healthMultiplier,
                modifier.damageMultiplier,
                modifier.speedMultiplier,
                modifier.lootBonusChance
            );
        }
    }

    /// Restores original enemy stats after special waves.
    void RestoreOriginalEnemyStats()
    {
        if (originalStats.Count == 0) return;

        foreach (var kvp in originalStats)
        {
            string enemyName = kvp.Key;
            var (health, damage, speed, lootDropChance) = kvp.Value;

            EnemyType enemyType = availableEnemyTypes.Find(e => e.name == enemyName);
            if (enemyType == null) continue;

            enemyType.health = health;
            enemyType.damage = damage;
            enemyType.speed = speed;
            enemyType.lootDropChance = lootDropChance;
        }

        originalStats.Clear();
    }
    #endregion

    #region Enemy Callbacks
    /// Called when an enemy dies.
    void OnEnemyDeath(GameObject enemy)
    {
        if (enemy == null) return;

        if (activeEnemies.Remove(enemy))
        {
            enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        }

        EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
        if (behavior != null)
        {
            if (behavior.IsBoss)
            {
                DropBossLoot(enemy.transform.position, behavior.BossLootTableName);
            }
            else
            {
                TryDropEnemyLoot(enemy.transform.position, behavior);
            }
        }
    }

    /// Safety check to remove null enemies from tracking.
    void SafetyPruneDeadEnemies()
    {
        int removedCount = 0;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                removedCount++;

                if (enemiesRemaining > 0)
                {
                    enemiesRemaining--;
                }
            }
        }

        if (removedCount > 0 && logWaveCompositions)
        {
            Debug.Log($"[WaveManager] Pruned {removedCount} null enemies from tracking");
        }
    }
    #endregion

    #region Loot System
    /// Drops boss loot at the specified position.
    void DropBossLoot(Vector3 position, string lootTableName)
    {
        if (LootSystem.Instance == null)
        {
            Debug.LogWarning("WaveManager: LootSystem not found! Boss loot cannot be dropped.");
            return;
        }

        string table = string.IsNullOrEmpty(lootTableName) ? DEFAULT_BOSS_LOOT_TABLE : lootTableName;
        LootSystem.Instance.DropBossLoot(position, table);
    }

    /// Attempts to drop loot from a regular enemy based on drop chance.
    void TryDropEnemyLoot(Vector3 position, EnemyBehavior behavior)
    {
        if (LootSystem.Instance == null)
        {
            Debug.LogWarning("WaveManager: LootSystem not found! Regular enemy loot cannot be dropped.");
            return;
        }

        if (string.IsNullOrEmpty(behavior.LootTableName))
        {
            return;
        }

        float dropChance = behavior.LootDropChance;
        if (dropChance <= 0f)
        {
            return;
        }

        if (UnityEngine.Random.value <= dropChance)
        {
            LootSystem.Instance.DropLoot(position, behavior.LootTableName);
        }
    }
    #endregion

    #region Build Phase
    /// Starts the build phase after a wave is complete.
    public void StartBuildPhase()
    {
        if (!useBuildPhases)
        {
            // Skip build phase and go directly to next wave
            if (timeBetweenWaves > 0f)
            {
                StartCoroutine(StartNextWaveAfterDelay(timeBetweenWaves));
            }
            else
            {
                currentWave++;
                StartNextWave();
            }
            return;
        }

        buildPhaseActive = true;
        isWaveActive = false;

        OnBuildPhaseStarted?.Invoke(currentWave);

        // Show build panel
        var bpc = FindObjectOfType<BuildPhaseController>();
        if (bpc != null)
        {
            bpc.ShowBuildPanel();
        }
        else
        {
            Debug.LogWarning("[WaveManager] BuildPhaseController not found! Cannot show build panel.");
        }

        Debug.Log($"[WaveManager] Build Phase started after wave {currentWave}.");
    }

    /// Finishes the build phase and starts the next wave.
    public void FinishBuildPhase()
    {
        if (currentWave >= maxWaves)
        {
            Debug.LogWarning("WaveManager: Cannot start next wave - max waves reached");
            OnAllWavesComplete?.Invoke();
            return;
        }

        if (isWaveActive)
        {
            Debug.LogWarning("WaveManager: Cannot finish build phase - wave is still active");
            return;
        }

        Debug.Log($"[WaveManager] Build Phase finished. Preparing wave {currentWave + 1}");

        buildPhaseActive = false;

        if (timeBetweenWaves > 0f)
        {
            StartCoroutine(StartNextWaveAfterDelay(timeBetweenWaves));
        }
        else
        {
            currentWave++;
            StartNextWave();
        }
    }

    /// Waits for a delay before starting the next wave.
    IEnumerator StartNextWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentWave++;
        StartNextWave();
    }
    #endregion

    #region Stronghold
    /// Damages the stronghold by the specified amount.
    public void DamageStronghold(int amount)
    {
        if (amount <= 0) return;

        if (!stronghold)
        {
            Debug.LogWarning("WaveManager: DamageStronghold called but no StrongholdHealth is assigned.");
            return;
        }

        stronghold.TakeDamage(amount);

        if (stronghold.CurrentHealth <= 0)
        {
            StopCurrentWave();

            if (GameManager.Instance)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
    #endregion

    #region Utility Methods
    /// Resolves the player anchor transform for enemy targeting.
    Transform ResolvePlayerAnchor()
    {
        if (playerDetectionAnchor != null)
        {
            return playerDetectionAnchor;
        }

        if (cachedPlayerAnchor != null)
        {
            return cachedPlayerAnchor;
        }

        if (!hasSearchedForPlayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
            {
                cachedPlayerAnchor = playerObj.transform;
            }
            hasSearchedForPlayer = true;
        }

        return cachedPlayerAnchor;
    }

    /// <summary>
    /// Gets the enemy type for a faction entry.
    /// </summary>
    EnemyType GetEnemyTypeForEntry(FactionEnemyEntry entry)
    {
        string key = ResolveEntryName(entry);
        EnemyType enemyType = availableEnemyTypes.Find(e => e.name == key);

        if (enemyType == null)
        {
            enemyType = entry.ToEnemyType();
            availableEnemyTypes.Add(enemyType);
        }

        return enemyType;
    }

    /// Resolves the display name for a faction enemy entry.
    string ResolveEntryName(FactionEnemyEntry entry)
    {
        if (entry == null) return DEFAULT_ENEMY_NAME;
        if (!string.IsNullOrEmpty(entry.displayName)) return entry.displayName;
        if (entry.prefab != null) return entry.prefab.name;
        return DEFAULT_ENEMY_NAME;
    }

    /// Builds a unique signature for a wave composition.
    string BuildWaveSignature(WaveComposition wave)
    {
        if (wave == null || wave.enemies == null || wave.enemies.Count == 0)
        {
            return string.Empty;
        }

        var parts = wave.enemies
            .OrderBy(e => e.enemyTypeName)
            .Select(e => $"{e.enemyTypeName}:{e.count}");

        return string.Join("|", parts);
    }

    /// Checks if a wave signature is a duplicate.
    bool IsDuplicateSignature(string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            return false;
        }

        if (signature == lastWaveSignature)
        {
            return true;
        }

        return generatedWaveSignatures.Contains(signature);
    }

    /// Shuffles a list using the Fisher-Yates algorithm.
    void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }

    /// Gets a seeded random number generator for the wave.
    System.Random GetWaveRandom(int waveNumber)
    {
        if (RunState.Instance != null)
        {
            return RunState.Instance.CreateWaveRandom(waveNumber);
        }

        return new System.Random(fallbackRandom.Next());
    }

    /// Logs the composition of a generated wave.
    void LogWaveComposition(int waveNumber, WaveComposition wave, int targetBudget, int usedBudget)
    {
        string composition = string.Join(", ", wave.enemies.Select(e => $"{e.enemyTypeName} x{e.count}"));
        int seed = RunState.Instance != null ? RunState.Instance.RunSeed : 0;
        Debug.Log($"[WaveManager] Wave {waveNumber} (seed {seed}) targetBudget={targetBudget}, usedBudget={usedBudget}: {composition}");
    }
    #endregion
}
