using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactionEnemyPool", menuName = "Waves/Faction Enemy Pool", order = 0)]
public class FactionEnemyPool : ScriptableObject
{
    public string factionName = "Default";
    public List<FactionEnemyEntry> enemies = new List<FactionEnemyEntry>();

    public IEnumerable<FactionEnemyEntry> GetEntriesForWave(int waveNumber)
    {
        foreach (var entry in enemies)
        {
            if (entry == null) continue;
            if (entry.IsAvailableForWave(waveNumber))
            {
                yield return entry;
            }
        }
    }
}

[System.Serializable]
public class FactionEnemyEntry
{
    [Header("Enemy Data")]
    public string displayName = "Enemy";
    public GameObject prefab;
    public int health = 50;
    public float speed = 3f;
    public int damage = 10;
    public bool isBoss = false;

    [Header("Loot Settings")]
    public string lootTableName = "BasicEnemy";
    [Range(0f, 1f)] public float lootDropChance = 0.1f;
    [Tooltip("Override loot table when this enemy is marked as a boss.")]
    public string bossLootTableName = "BasicBoss";

    [Header("Spawn Rules")]
    [Range(0.05f, 10f)] public float spawnWeight = 1f;
    public int minWave = 1;
    public int maxWave = 999;
    public int maxPerWave = -1;

    [Tooltip("Adds extra difficulty cost for this enemy. Higher cost reduces how many can appear per wave.")]
    public int difficultyCost = 1;

    public bool IsAvailableForWave(int waveNumber)
    {
        return waveNumber >= minWave && waveNumber <= maxWave;
    }

    public EnemyType ToEnemyType()
    {
        return new EnemyType
        {
            name = string.IsNullOrEmpty(displayName) ? (prefab != null ? prefab.name : "Enemy") : displayName,
            prefab = prefab,
            health = health,
            speed = speed,
            damage = damage,
            spawnWeight = spawnWeight,
            isBoss = isBoss,
            lootTableName = lootTableName,
            lootDropChance = lootDropChance,
            bossLootTableName = bossLootTableName
        };
    }
}
