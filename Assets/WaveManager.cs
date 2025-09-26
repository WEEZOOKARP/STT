using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;
    public int health;
    public float speed;
    public int damage;
    public float spawnWeight = 1f; // Higher weight = more likely to spawn
    public bool isBoss = false;
    public string lootTableName;
    public float lootDropChance;
    public string bossLootTableName;

    // (These two fields seem unrelated to EnemyType, but keeping them here
    //  since they existed in your version. Remove if not needed.)
    public int strongholdMaxHealth = 300;
    public int strongholdCurrentHealth;
}

[System.Serializable]
public class WaveComposition
{
    public int waveNumber;
    public List<EnemySpawn> enemies;
    public bool isBossWave = false;
    public float waveDuration = 60f; // How long the wave lasts
}

[System.Serializable]
public class EnemySpawn
{
    public string enemyTypeName;
    public int count;
    public float spawnDelay;
    public Vector3 spawnPosition;
}

public class WaveManager : MonoBehaviour
{
    [Header("Build Phases")]
    public bool useBuildPhases = true;
    [HideInInspector] public bool waitingForBuild = false;

    [SerializeField] private StrongholdHealth stronghold;
    public int strongholdCurrentHealth => stronghold ? stronghold.CurrentHealth : 0;
    public int strongholdMaxHealth => stronghold ? stronghold.MaxHealth : 0;

    // Event for tutorial condition subscribed to wave completion event - Archie | [25/09/25].
    public static event System.Action<int> OnWaveCompleted;

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

    [Header("Targets")]
    [Tooltip("Stronghold or base the enemies march toward when not pursuing the player.")]
    public Transform strongholdTarget;
    [Tooltip("Optional detection anchor (e.g. invisible child object) attached to the player.")]
    public Transform playerDetectionAnchor;

    [Header("State")]
    public int currentWave = 1;
    public bool isWaveActive = false;
    public int enemiesRemaining = 0;
    public bool buildPhaseActive = false; // NEW

    [Header("Debug")]
    [SerializeField] private bool autoStartOnAwake = false;
    [SerializeField] private bool logWaveCompositions = false;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private readonly Dictionary<string, FactionEnemyEntry> entryLookup = new Dictionary<string, FactionEnemyEntry>();
    private Coroutine currentWaveCoroutine;
    private System.Random fallbackRandom = new System.Random();
    private readonly HashSet<string> generatedWaveSignatures = new HashSet<string>();
    private string lastWaveSignature;

    private const int MaxWaveRerollAttempts = 5;

    // Events
    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveComplete;
    public System.Action OnAllWavesComplete;
    public System.Action<int> OnBuildPhaseStarted; // NEW

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
            RegisterEnemyEntry(new FactionEnemyEntry
            {
                displayName = "Basic Enemy",
                health = 50,
                speed = 3f,
                damage = 10,
                spawnWeight = 1f,
                difficultyCost = 1,
                lootTableName = "BasicEnemy",
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
                lootTableName = "BasicBoss",
                lootDropChance = 1f,
                bossLootTableName = "BasicBoss"
            });
        }
    }

    public void BeginRun(int startingWave = 1)
    {
        InitializeEnemyTypes();
        ResetState();
        currentWave = Mathf.Max(1, startingWave);
        if (stronghold) stronghold.ResetHealth();

        StartNextWave();
    }

    void RegisterEnemyEntry(FactionEnemyEntry entry)
    {
        if (entry == null) return;
        EnemyType type = entry.ToEnemyType();
        string key = ResolveEntryName(entry);

        if (!entryLookup.ContainsKey(key))
        {
            entryLookup.Add(key, entry);
        }
        else
        {
            entryLookup[key] = entry;
        }

        availableEnemyTypes.Add(type);
    }

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
        buildPhaseActive = false; // NEW
        generatedWaveSignatures.Clear();
        lastWaveSignature = null;
    }

    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] StartNextWave() -> wave {currentWave}");
        if (currentWave > maxWaves) { OnAllWavesComplete?.Invoke(); return; }
        buildPhaseActive = false;
        currentWaveCoroutine = StartCoroutine(RunWave(currentWave));
    }

    IEnumerator RunWave(int waveNumber)
    {
        Debug.Log($"[WaveManager] RunWave({waveNumber}) starting");
        isWaveActive = true;
        OnWaveStart?.Invoke(waveNumber);

        System.Random waveRandom = GetWaveRandom(waveNumber);

        // Generate randomized wave composition
        WaveComposition waveComp = GenerateRandomWave(waveNumber, waveRandom);

        // Spawn enemies
        yield return StartCoroutine(SpawnWaveEnemies(waveComp, waveRandom));

        
        while (true)
        {
            PruneDeadEnemies(); // keep the list clean

            // exit when counter says zero OR when there are simply no tracked enemies left
            if (enemiesRemaining <= 0 || activeEnemies.Count == 0)
                break;

            yield return null;
        }


        // Wave complete
        isWaveActive = false;
        OnWaveComplete?.Invoke(waveNumber);
        OnWaveCompleted?.Invoke(waveNumber); // Tutorial event - Archie | [25/09/25]

        // Track wave completion in meta progression
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.CompleteWave(waveNumber);
        }

        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
        StartNextWave();
    }

    
    void PruneDeadEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
                // if something killed an enemy without calling OnDeath, make sure we don't get stuck
                if (enemiesRemaining > 0) enemiesRemaining--;
            }
        }
    }


    // ---------- BUILD PHASE CONTROL (NEW) ----------
    public void StartBuildPhase()
    {
        buildPhaseActive = true;
        isWaveActive = false;

        // fire the event (keeps things decoupled if you use it)
        OnBuildPhaseStarted?.Invoke(currentWave);

        // ✅ hard-call the controller so the panel always shows
        var bpc = FindObjectOfType<BuildPhaseController>();
        if (bpc != null)
        {
            bpc.ShowBuildPanel();
        }

        Debug.Log($"[WaveManager] Build Phase started after wave {currentWave}.");
    }

    /// <summary>
    /// Call this from your UI "Finish" button to end the build phase and start the next wave.
    /// </summary>
    public void FinishBuildPhase()
    {
        // Log for sanity
        Debug.Log($"[WaveManager] FinishBuildPhase() pressed. buildPhaseActive={buildPhaseActive}, currentWave={currentWave}");

        // Don’t gate behind buildPhaseActive – just proceed
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

    private IEnumerator StartNextWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentWave++;
        StartNextWave();
    }
    // ------------------------------------------------

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
            Debug.LogWarning("WaveManager: No eligible enemies found for wave " + waveNumber);
            return wave;
        }

        bool isBossWave = bossWaveInterval > 0 && waveNumber % bossWaveInterval == 0 && eligibleEntries.Any(e => e.isBoss);
        wave.isBossWave = isBossWave;

        Dictionary<FactionEnemyEntry, int> counts = new Dictionary<FactionEnemyEntry, int>();
        int targetBudget = CalculateWaveBudget(waveNumber, rng, isBossWave);
        int usedBudget = 0;

        if (isBossWave)
        {
            var bossEntry = PickEnemyEntry(eligibleEntries, rng, counts, int.MaxValue, true);
            if (bossEntry != null)
            {
                RegisterSpawn(bossEntry, counts);
                usedBudget += Mathf.Max(1, bossEntry.difficultyCost);
            }
        }

        int safety = 0;
        while (usedBudget < targetBudget && safety < 500)
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

        string signature = BuildWaveSignature(wave);
        if (IsDuplicateSignature(signature) && rerollDepth < MaxWaveRerollAttempts)
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

    List<FactionEnemyEntry> GatherEligibleEntries(int waveNumber)
    {
        List<FactionEnemyEntry> entries = new List<FactionEnemyEntry>();

        foreach (var pair in entryLookup)
        {
            FactionEnemyEntry entry = pair.Value;
            if (entry == null) continue;
            if (entry.IsAvailableForWave(waveNumber))
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

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

    void SpawnEnemy(string enemyTypeName, System.Random rng)
    {
        EnemyType enemyType = availableEnemyTypes.Find(e => e.name == enemyTypeName);
        if (enemyType == null)
        {
            Debug.LogWarning("WaveManager: Enemy type not found for name " + enemyTypeName);
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
        enemy.tag = "Enemy";

        EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
        if (behavior == null)
        {
            behavior = enemy.AddComponent<EnemyBehavior>();
        }

        behavior.Initialize(enemyType);
        behavior.ConfigureTargets(strongholdTarget, ResolvePlayerAnchor());
        behavior.OnDeath -= OnEnemyDeath;
        behavior.OnDeath += OnEnemyDeath;

        activeEnemies.Add(enemy);
    }

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

    Transform ResolvePlayerAnchor()
    {
        if (playerDetectionAnchor != null)
        {
            return playerDetectionAnchor;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj != null ? playerObj.transform : null;
    }

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

    string ResolveEntryName(FactionEnemyEntry entry)
    {
        if (entry == null) return "Enemy";
        if (!string.IsNullOrEmpty(entry.displayName)) return entry.displayName;
        if (entry.prefab != null) return entry.prefab.name;
        return "Enemy";
    }

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

    System.Random GetWaveRandom(int waveNumber)
    {
        if (RunState.Instance != null)
        {
            return RunState.Instance.CreateWaveRandom(waveNumber);
        }

        return new System.Random(fallbackRandom.Next());
    }

    void LogWaveComposition(int waveNumber, WaveComposition wave, int targetBudget, int usedBudget)
    {
        string composition = string.Join(", ", wave.enemies.Select(e => $"{e.enemyTypeName} x{e.count}"));
        int seed = RunState.Instance != null ? RunState.Instance.RunSeed : 0;
        Debug.Log($"Wave {waveNumber} (seed {seed}) targetBudget={targetBudget}, usedBudget={usedBudget}: {composition}");
    }

    void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesRemaining--;

        // Check if this was a boss
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

    void DropBossLoot(Vector3 position, string lootTableName)
    {
        // Use the loot system to drop boss loot
        if (LootSystem.Instance != null)
        {
            string table = string.IsNullOrEmpty(lootTableName) ? "BasicBoss" : lootTableName;
            LootSystem.Instance.DropBossLoot(position, table);
        }
        else
        {
            Debug.LogWarning("LootSystem not found! Boss loot cannot be dropped.");
        }
    }

    void TryDropEnemyLoot(Vector3 position, EnemyBehavior behavior)
    {
        if (LootSystem.Instance == null)
        {
            Debug.LogWarning("LootSystem not found! Regular enemy loot cannot be dropped.");
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

    public void StopCurrentWave()
    {
        ResetState();
    }

    public void DamageStronghold(int amount)
    {
        if (amount <= 0) return;

        if (!stronghold)
        {
            Debug.LogWarning("DamageStronghold called but no StrongholdHealth is assigned.");
            return;
        }

        stronghold.TakeDamage(amount);

        if (stronghold.CurrentHealth <= 0)
        {
            // stop waves and end the game
            StopCurrentWave();
            if (GameManager.Instance) GameManager.Instance.GameOver();
        }
    }
}
