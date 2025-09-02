using System.Collections;
using System.Collections.Generic;
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
    [Header("Enemy Types")]
    public List<EnemyType> availableEnemyTypes = new List<EnemyType>();
    
    [Header("Wave Settings")]
    public List<WaveComposition> predefinedWaves = new List<WaveComposition>();
    public int maxWaves = 10;
    public float timeBetweenWaves = 5f;
    
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnRadius = 10f;
    
    [Header("Current Wave")]
    public int currentWave = 1;
    public bool isWaveActive = false;
    public int enemiesRemaining = 0;
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine currentWaveCoroutine;
    
    // Events
    public System.Action<int> OnWaveStart;
    public System.Action<int> OnWaveComplete;
    public System.Action OnAllWavesComplete;
    
    void Start()
    {
        InitializeEnemyTypes();
        StartNextWave();
    }
    
    void InitializeEnemyTypes()
    {
        // Create default enemy types if none are set
        if (availableEnemyTypes.Count == 0)
        {
            availableEnemyTypes.Add(new EnemyType { 
                name = "Basic Enemy", 
                health = 50, 
                speed = 3f, 
                damage = 10, 
                spawnWeight = 1f 
            });
            
            availableEnemyTypes.Add(new EnemyType { 
                name = "Fast Enemy", 
                health = 30, 
                speed = 5f, 
                damage = 5, 
                spawnWeight = 0.7f 
            });
            
            availableEnemyTypes.Add(new EnemyType { 
                name = "Tank Enemy", 
                health = 100, 
                speed = 2f, 
                damage = 15, 
                spawnWeight = 0.5f 
            });
            
            availableEnemyTypes.Add(new EnemyType { 
                name = "Boss", 
                health = 200, 
                speed = 2.5f, 
                damage = 25, 
                spawnWeight = 0.1f, 
                isBoss = true 
            });
        }
    }
    
    public void StartNextWave()
    {
        if (currentWave > maxWaves)
        {
            OnAllWavesComplete?.Invoke();
            return;
        }
        
        currentWaveCoroutine = StartCoroutine(RunWave(currentWave));
    }
    
    IEnumerator RunWave(int waveNumber)
    {
        isWaveActive = true;
        OnWaveStart?.Invoke(waveNumber);
        
        // Generate randomized wave composition
        WaveComposition waveComp = GenerateRandomWave(waveNumber);
        
        // Spawn enemies
        yield return StartCoroutine(SpawnWaveEnemies(waveComp));
        
        // Wait for all enemies to be defeated
        while (enemiesRemaining > 0)
        {
            yield return null;
        }
        
        // Wave complete
        isWaveActive = false;
        OnWaveComplete?.Invoke(waveNumber);
        
        // Track wave completion in meta progression
        if (MetaProgression.Instance != null)
        {
            MetaProgression.Instance.CompleteWave(waveNumber);
        }
        
        // Wait before next wave
        yield return new WaitForSeconds(timeBetweenWaves);
        
        currentWave++;
        StartNextWave();
    }
    
    WaveComposition GenerateRandomWave(int waveNumber)
    {
        WaveComposition wave = new WaveComposition();
        wave.waveNumber = waveNumber;
        wave.enemies = new List<EnemySpawn>();
        
        // Determine if this is a boss wave (every 5th wave)
        wave.isBossWave = (waveNumber % 5 == 0);
        
        if (wave.isBossWave)
        {
            // Boss wave - spawn 1 boss + some regular enemies
            EnemyType bossType = GetRandomEnemyType(true);
            wave.enemies.Add(new EnemySpawn { 
                enemyTypeName = bossType.name, 
                count = 1, 
                spawnDelay = 0f 
            });
            
            // Add some regular enemies
            int regularEnemyCount = Random.Range(3, 8);
            for (int i = 0; i < regularEnemyCount; i++)
            {
                EnemyType regularType = GetRandomEnemyType(false);
                wave.enemies.Add(new EnemySpawn { 
                    enemyTypeName = regularType.name, 
                    count = 1, 
                    spawnDelay = Random.Range(2f, 8f) 
                });
            }
        }
        else
        {
            // Regular wave - spawn mix of enemies
            int totalEnemies = Random.Range(5 + waveNumber, 10 + waveNumber * 2);
            int enemyTypes = Mathf.Min(availableEnemyTypes.Count, Random.Range(2, 4));
            
            for (int i = 0; i < totalEnemies; i++)
            {
                EnemyType enemyType = GetRandomEnemyType(false);
                wave.enemies.Add(new EnemySpawn { 
                    enemyTypeName = enemyType.name, 
                    count = 1, 
                    spawnDelay = Random.Range(1f, 3f) 
                });
            }
        }
        
        return wave;
    }
    
    EnemyType GetRandomEnemyType(bool bossOnly = false)
    {
        List<EnemyType> validTypes = new List<EnemyType>();
        
        foreach (EnemyType type in availableEnemyTypes)
        {
            if (bossOnly && type.isBoss) validTypes.Add(type);
            else if (!bossOnly && !type.isBoss) validTypes.Add(type);
        }
        
        if (validTypes.Count == 0) return availableEnemyTypes[0];
        
        // Weighted random selection
        float totalWeight = 0f;
        foreach (EnemyType type in validTypes)
        {
            totalWeight += type.spawnWeight;
        }
        
        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (EnemyType type in validTypes)
        {
            currentWeight += type.spawnWeight;
            if (random <= currentWeight)
            {
                return type;
            }
        }
        
        return validTypes[0];
    }
    
    IEnumerator SpawnWaveEnemies(WaveComposition wave)
    {
        enemiesRemaining = 0;
        
        foreach (EnemySpawn spawn in wave.enemies)
        {
            yield return new WaitForSeconds(spawn.spawnDelay);
            
            for (int i = 0; i < spawn.count; i++)
            {
                SpawnEnemy(spawn.enemyTypeName);
                enemiesRemaining++;
                yield return new WaitForSeconds(0.5f); // Small delay between spawns
            }
        }
    }
    
    void SpawnEnemy(string enemyTypeName)
    {
        EnemyType enemyType = availableEnemyTypes.Find(e => e.name == enemyTypeName);
        if (enemyType == null) return;
        
        // Get random spawn position
        Vector3 spawnPos = GetRandomSpawnPosition();
        
        // Create enemy GameObject (placeholder - you'll need to create actual prefabs)
        GameObject enemy = new GameObject(enemyType.name);
        enemy.transform.position = spawnPos;
        enemy.tag = "Enemy";
        
        // Add enemy behavior component
        EnemyBehavior behavior = enemy.AddComponent<EnemyBehavior>();
        behavior.Initialize(enemyType);
        
        activeEnemies.Add(enemy);
        
        // Subscribe to enemy death
        behavior.OnDeath += OnEnemyDeath;
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position + Random.insideUnitSphere * spawnRadius;
        }
        
        // Fallback to random position around origin
        return Random.insideUnitSphere * spawnRadius;
    }
    
    void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesRemaining--;
        
        // Check if this was a boss
        EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
        if (behavior != null && behavior.IsBoss)
        {
            // Trigger boss loot drop
            DropBossLoot(enemy.transform.position);
        }
    }
    
    void DropBossLoot(Vector3 position)
    {
        // Use the loot system to drop boss loot
        if (LootSystem.Instance != null)
        {
            LootSystem.Instance.DropBossLoot(position, "BasicBoss");
        }
        else
        {
            Debug.LogWarning("LootSystem not found! Boss loot cannot be dropped.");
        }
    }
    
    public void StopCurrentWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
        }
        
        // Destroy all active enemies
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
    }
}
