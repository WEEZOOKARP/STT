using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LootRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[System.Serializable]
public class LootItem
{
    public string name;
    public string description;
    public LootRarity rarity;
    public Sprite icon;
    public GameObject prefab;
    
    // Stats
    public int damageBonus = 0;
    public int healthBonus = 0;
    public float speedBonus = 0f;
    public int ammoBonus = 0;
    public float criticalChanceBonus = 0f;
    public float criticalDamageBonus = 0f;
    
    // Special effects
    public bool hasLifeSteal = false;
    public bool hasExplosiveRounds = false;
    public bool hasPiercingShots = false;
    public bool hasRapidFire = false;
    public bool hasShieldGenerator = false;
    
    // Meta progression rewards
    public int metaCurrencyReward = 0;
    public int skillPointsReward = 0;
    public float experienceReward = 0f;
    
    public LootItem(string name, string description, LootRarity rarity)
    {
        this.name = name;
        this.description = description;
        this.rarity = rarity;
    }
}

[System.Serializable]
public class LootTable
{
    public string tableName;
    public List<LootDrop> possibleDrops = new List<LootDrop>();
    public int guaranteedDrops = 1;
    public int maxRandomDrops = 3;
    public float dropChance = 1f; // Chance for additional random drops
}

[System.Serializable]
public class LootDrop
{
    public LootItem item;
    public float dropChance = 0.1f; // 10% base chance
    public int minQuantity = 1;
    public int maxQuantity = 1;
}

public class LootSystem : MonoBehaviour
{
    public static LootSystem Instance { get; private set; }
    
    [Header("Loot Settings")]
    public List<LootItem> allLootItems = new List<LootItem>();
    public List<LootTable> lootTables = new List<LootTable>();
    public GameObject lootDropPrefab;
    public float lootDropForce = 5f;
    public float lootDropRadius = 3f;
    
    [Header("Rarity Colors")]
    public Color commonColor = Color.white;
    public Color uncommonColor = Color.green;
    public Color rareColor = Color.blue;
    public Color epicColor = Color.magenta;
    public Color legendaryColor = Color.yellow;
    
    [Header("Boss Loot Multipliers")]
    public float bossLootMultiplier = 2f;
    public float legendaryDropChanceMultiplier = 1.5f;
    
    private Dictionary<string, LootTable> lootTableLookup = new Dictionary<string, LootTable>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLootTables();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeLootTables()
    {
        // Create default loot items if none exist
        if (allLootItems.Count == 0)
        {
            CreateDefaultLootItems();
        }
        
        // Create default loot tables
        if (lootTables.Count == 0)
        {
            CreateDefaultLootTables();
        }
        
        // Build lookup dictionary
        foreach (LootTable table in lootTables)
        {
            lootTableLookup[table.tableName] = table;
        }
    }
    
    void CreateDefaultLootItems()
    {
        // Common items - basic consumables
        allLootItems.Add(new LootItem("Health Potion", "Restores 50 health", LootRarity.Common)
        {
            healthBonus = 50,
            metaCurrencyReward = 10
        });
        
        allLootItems.Add(new LootItem("Ammo Pack", "Provides extra ammunition", LootRarity.Common)
        {
            ammoBonus = 20,
            metaCurrencyReward = 15
        });
        
        allLootItems.Add(new LootItem("Energy Drink", "Temporary speed boost", LootRarity.Common)
        {
            speedBonus = 1f,
            metaCurrencyReward = 12
        });
        
        // Uncommon items - basic equipment
        allLootItems.Add(new LootItem("Combat Knife", "Sharp blade for close combat", LootRarity.Uncommon)
        {
            damageBonus = 15,
            speedBonus = 0.5f,
            metaCurrencyReward = 25
        });
        
        allLootItems.Add(new LootItem("Reinforced Armor", "Provides additional protection", LootRarity.Uncommon)
        {
            healthBonus = 25,
            metaCurrencyReward = 30
        });
        
        allLootItems.Add(new LootItem("Tactical Scope", "Improved accuracy", LootRarity.Uncommon)
        {
            criticalChanceBonus = 0.1f,
            metaCurrencyReward = 28
        });
        
        // Rare items - special effects
        allLootItems.Add(new LootItem("Vampiric Blade", "Steals life from enemies", LootRarity.Rare)
        {
            damageBonus = 25,
            hasLifeSteal = true,
            metaCurrencyReward = 50,
            skillPointsReward = 1
        });
        
        allLootItems.Add(new LootItem("Explosive Rounds", "Bullets explode on impact", LootRarity.Rare)
        {
            damageBonus = 20,
            hasExplosiveRounds = true,
            metaCurrencyReward = 60,
            skillPointsReward = 1
        });
        
        allLootItems.Add(new LootItem("Piercing Ammo", "Bullets go through enemies", LootRarity.Rare)
        {
            damageBonus = 18,
            hasPiercingShots = true,
            metaCurrencyReward = 55,
            skillPointsReward = 1
        });
        
        // Epic items - powerful equipment
        allLootItems.Add(new LootItem("Plasma Rifle", "Advanced energy weapon", LootRarity.Epic)
        {
            damageBonus = 40,
            ammoBonus = 30,
            hasRapidFire = true,
            metaCurrencyReward = 100,
            skillPointsReward = 2,
            experienceReward = 50f
        });
        
        allLootItems.Add(new LootItem("Shield Generator", "Creates protective barrier", LootRarity.Epic)
        {
            healthBonus = 50,
            hasShieldGenerator = true,
            metaCurrencyReward = 120,
            skillPointsReward = 2,
            experienceReward = 75f
        });
        
        allLootItems.Add(new LootItem("Quantum Boots", "Enhanced mobility", LootRarity.Epic)
        {
            speedBonus = 2f,
            criticalChanceBonus = 0.15f,
            metaCurrencyReward = 110,
            skillPointsReward = 2,
            experienceReward = 60f
        });
        
        // Legendary items - game-changing equipment
        allLootItems.Add(new LootItem("Dragon's Breath", "Legendary weapon of destruction", LootRarity.Legendary)
        {
            damageBonus = 75,
            criticalChanceBonus = 0.25f,
            criticalDamageBonus = 0.5f,
            hasExplosiveRounds = true,
            hasPiercingShots = true,
            metaCurrencyReward = 200,
            skillPointsReward = 5,
            experienceReward = 150f
        });
        
        allLootItems.Add(new LootItem("Phoenix Armor", "Resurrects you once per game", LootRarity.Legendary)
        {
            healthBonus = 100,
            speedBonus = 1f,
            hasShieldGenerator = true,
            metaCurrencyReward = 250,
            skillPointsReward = 5,
            experienceReward = 200f
        });
        
        allLootItems.Add(new LootItem("Chronos Gauntlets", "Manipulates time itself", LootRarity.Legendary)
        {
            damageBonus = 60,
            speedBonus = 1.5f,
            criticalChanceBonus = 0.3f,
            hasRapidFire = true,
            metaCurrencyReward = 300,
            skillPointsReward = 5,
            experienceReward = 250f
        });
    }
    
    void CreateDefaultLootTables()
    {
        // Basic Enemy Loot Table - minimal drops, mostly common items
        LootTable basicEnemyTable = new LootTable
        {
            tableName = "BasicEnemy",
            guaranteedDrops = 0,
            maxRandomDrops = 1,
            dropChance = 0.25f // 25% chance to drop anything
        };

        // Only add common and uncommon items to basic enemies
        foreach (LootItem item in allLootItems)
        {
            if (item.rarity <= LootRarity.Uncommon)
            {
                float baseChance = GetRarityDropChance(item.rarity);
                basicEnemyTable.possibleDrops.Add(new LootDrop
                {
                    item = item,
                    dropChance = baseChance,
                    minQuantity = 1,
                    maxQuantity = 1
                });
            }
        }

        lootTables.Add(basicEnemyTable);

        // Fast Enemy Loot Table - speed-focused drops
        LootTable fastEnemyTable = new LootTable
        {
            tableName = "FastEnemy",
            guaranteedDrops = 0,
            maxRandomDrops = 1,
            dropChance = 0.35f
        };

        // Fast enemies drop speed-related items more often
        foreach (LootItem item in allLootItems)
        {
            float baseChance = GetRarityDropChance(item.rarity);
            
            // Boost speed-related items
            if (item.speedBonus > 0 || item.name.Contains("Energy") || item.name.Contains("Quantum"))
            {
                baseChance *= 2f;
            }
            
            // Limit to uncommon and below for regular enemies
            if (item.rarity <= LootRarity.Uncommon)
            {
                fastEnemyTable.possibleDrops.Add(new LootDrop
                {
                    item = item,
                    dropChance = baseChance,
                    minQuantity = 1,
                    maxQuantity = 1
                });
            }
        }

        lootTables.Add(fastEnemyTable);

        // Tank Enemy Loot Table - defense-focused drops
        LootTable tankEnemyTable = new LootTable
        {
            tableName = "TankEnemy",
            guaranteedDrops = 0,
            maxRandomDrops = 1,
            dropChance = 0.4f
        };

        // Tank enemies drop defense-related items more often
        foreach (LootItem item in allLootItems)
        {
            float baseChance = GetRarityDropChance(item.rarity);
            
            // Boost defense-related items
            if (item.healthBonus > 0 || item.name.Contains("Armor") || item.name.Contains("Shield"))
            {
                baseChance *= 2f;
            }
            
            // Limit to uncommon and below for regular enemies
            if (item.rarity <= LootRarity.Uncommon)
            {
                tankEnemyTable.possibleDrops.Add(new LootDrop
                {
                    item = item,
                    dropChance = baseChance,
                    minQuantity = 1,
                    maxQuantity = 1
                });
            }
        }

        lootTables.Add(tankEnemyTable);

        // Basic Boss Loot Table - guaranteed good drops
        LootTable basicBossTable = new LootTable
        {
            tableName = "BasicBoss",
            guaranteedDrops = 2,
            maxRandomDrops = 2,
            dropChance = 0.8f
        };
        
        // Bosses can drop rare+ items
        foreach (LootItem item in allLootItems)
        {
            float baseChance = GetRarityDropChance(item.rarity);
            
            // Boost rare+ items for bosses
            if (item.rarity >= LootRarity.Rare)
            {
                baseChance *= 1.5f;
            }
            
            basicBossTable.possibleDrops.Add(new LootDrop
            {
                item = item,
                dropChance = baseChance,
                minQuantity = 1,
                maxQuantity = 1
            });
        }
        
        lootTables.Add(basicBossTable);
        
        // Elite Boss Loot Table - premium drops
        LootTable eliteBossTable = new LootTable
        {
            tableName = "EliteBoss",
            guaranteedDrops = 3,
            maxRandomDrops = 3,
            dropChance = 1f
        };
        
        // Elite bosses heavily favor rare+ items
        foreach (LootItem item in allLootItems)
        {
            float baseChance = GetRarityDropChance(item.rarity);
            
            if (item.rarity >= LootRarity.Rare)
            {
                baseChance *= legendaryDropChanceMultiplier * 2f; // Double boost for elite bosses
            }
            else if (item.rarity == LootRarity.Uncommon)
            {
                baseChance *= 0.5f; // Reduce common/uncommon drops
            }
            else
            {
                baseChance *= 0.2f; // Heavily reduce common drops
            }
            
            eliteBossTable.possibleDrops.Add(new LootDrop
            {
                item = item,
                dropChance = baseChance,
                minQuantity = 1,
                maxQuantity = 2
            });
        }
        
        lootTables.Add(eliteBossTable);
    }
    
    float GetRarityDropChance(LootRarity rarity)
    {
        switch (rarity)
        {
            case LootRarity.Common: return 0.4f;
            case LootRarity.Uncommon: return 0.25f;
            case LootRarity.Rare: return 0.15f;
            case LootRarity.Epic: return 0.08f;
            case LootRarity.Legendary: return 0.02f;
            default: return 0.1f;
        }
    }
    
    public void DropBossLoot(Vector3 position, string bossType = "BasicBoss")
    {
        DropLoot(position, bossType);
    }

    public void DropLoot(Vector3 position, string tableName)
    {
        LootTable lootTable = GetLootTable(tableName);
        if (lootTable == null)
        {
            Debug.LogWarning($"No loot table found for table: {tableName}");
            return;
        }

        List<LootItem> droppedItems = GenerateLootDrops(lootTable);

        // Spawn loot drops
        foreach (LootItem item in droppedItems)
        {
            SpawnLootDrop(item, position);
        }
        
        // Apply meta progression rewards
        ApplyMetaProgressionRewards(droppedItems);
        
        Debug.Log($"LootSystem: Dropped {droppedItems.Count} items from table '{tableName}'.");
    }
    
    LootTable GetLootTable(string tableName)
    {
        if (lootTableLookup.ContainsKey(tableName))
        {
            return lootTableLookup[tableName];
        }
        
        // Fallback to first available table
        return lootTables.Count > 0 ? lootTables[0] : null;
    }
    
    List<LootItem> GenerateLootDrops(LootTable lootTable)
    {
        List<LootItem> droppedItems = new List<LootItem>();
        
        // Guaranteed drops
        for (int i = 0; i < lootTable.guaranteedDrops; i++)
        {
            LootItem guaranteedItem = GetRandomLootItem(lootTable.possibleDrops);
            if (guaranteedItem != null)
            {
                droppedItems.Add(guaranteedItem);
            }
        }
        
        // Random drops
        int randomDrops = Random.Range(0, lootTable.maxRandomDrops + 1);
        for (int i = 0; i < randomDrops; i++)
        {
            if (Random.Range(0f, 1f) <= lootTable.dropChance)
            {
                LootItem randomItem = GetRandomLootItem(lootTable.possibleDrops);
                if (randomItem != null)
                {
                    droppedItems.Add(randomItem);
                }
            }
        }
        
        return droppedItems;
    }
    
    LootItem GetRandomLootItem(List<LootDrop> possibleDrops)
    {
        float totalWeight = 0f;
        foreach (LootDrop drop in possibleDrops)
        {
            totalWeight += drop.dropChance;
        }
        
        float random = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (LootDrop drop in possibleDrops)
        {
            currentWeight += drop.dropChance;
            if (random <= currentWeight)
            {
                return drop.item;
            }
        }
        
        return possibleDrops.Count > 0 ? possibleDrops[0].item : null;
    }
    
    void SpawnLootDrop(LootItem item, Vector3 position)
    {
        // Create loot drop GameObject
        GameObject lootDrop = new GameObject($"LootDrop_{item.name}");
        lootDrop.transform.position = position;
        
        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(lootDrop.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.5f;
        
        // Set color based on rarity
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = GetRarityColor(item.rarity);
        }
        
        // Add loot drop component
        LootDropBehavior dropBehavior = lootDrop.AddComponent<LootDropBehavior>();
        dropBehavior.Initialize(item);
        
        // Add physics for realistic drop
        Rigidbody rb = lootDrop.AddComponent<Rigidbody>();
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        rb.AddForce(randomDirection * lootDropForce, ForceMode.Impulse);
        
        // Add collider for pickup
        SphereCollider collider = lootDrop.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f;
        
        // Add pickup trigger
        lootDrop.AddComponent<LootPickupTrigger>();
    }
    
    Color GetRarityColor(LootRarity rarity)
    {
        switch (rarity)
        {
            case LootRarity.Common: return commonColor;
            case LootRarity.Uncommon: return uncommonColor;
            case LootRarity.Rare: return rareColor;
            case LootRarity.Epic: return epicColor;
            case LootRarity.Legendary: return legendaryColor;
            default: return Color.white;
        }
    }
    
    void ApplyMetaProgressionRewards(List<LootItem> items)
    {
        if (MetaProgression.Instance == null) return;
        
        int totalMetaCurrency = 0;
        int totalSkillPoints = 0;
        float totalExperience = 0f;
        
        foreach (LootItem item in items)
        {
            totalMetaCurrency += item.metaCurrencyReward;
            totalSkillPoints += item.skillPointsReward;
            totalExperience += item.experienceReward;
        }
        
        if (totalMetaCurrency > 0)
        {
            MetaProgression.Instance.AddMetaCurrency(totalMetaCurrency);
        }
        
        if (totalSkillPoints > 0)
        {
            MetaProgression.Instance.AddSkillPoints(totalSkillPoints);
        }
        
        if (totalExperience > 0)
        {
            MetaProgression.Instance.GainExperience(totalExperience);
        }
    }
    
    // Public methods for external use
    public void AddLootItem(LootItem item)
    {
        allLootItems.Add(item);
    }
    
    public void AddLootTable(LootTable table)
    {
        lootTables.Add(table);
        lootTableLookup[table.tableName] = table;
    }

    public List<LootItem> GetAllLootItems() => new List<LootItem>(allLootItems);
    public List<LootTable> GetAllLootTables() => new List<LootTable>(lootTables);
}

// Component for loot drop behavior
public class LootDropBehavior : MonoBehaviour
{
    public LootItem item;
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    private float bobTime;
    
    public void Initialize(LootItem lootItem)
    {
        item = lootItem;
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // Random start phase
    }
    
    void Update()
    {
        // Rotate the loot drop
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        bobTime += bobSpeed * Time.deltaTime;
        float bobOffset = Mathf.Sin(bobTime) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;
    }
}

// Component for loot pickup
public class LootPickupTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LootDropBehavior lootDrop = GetComponent<LootDropBehavior>();
            if (lootDrop != null && lootDrop.item != null)
            {
                PickupLoot(lootDrop.item);
                Destroy(gameObject);
            }
        }
    }
    
    void PickupLoot(LootItem item)
    {
        // Apply item effects to player
        ApplyItemEffects(item);
        
        // Show pickup notification
        ShowPickupNotification(item);
        
        Debug.Log($"Picked up: {item.name} ({item.rarity})");
    }
    
    void ApplyItemEffects(LootItem item)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Apply stat bonuses
        Status playerStatus = player.GetComponent<Status>();
        if (playerStatus != null && item.healthBonus > 0)
        {
            playerStatus.Heal(item.healthBonus);
        }
        
        // Apply to inventory
        Inventory playerInventory = player.GetComponent<Inventory>();
        if (playerInventory != null)
        {
            Item inventoryItem = new Item(item.name, item.description, 0, item.ammoBonus);
            playerInventory.AddItem(inventoryItem);
        }
        
        // Apply meta progression rewards
        if (MetaProgression.Instance != null)
        {
            if (item.metaCurrencyReward > 0)
            {
                MetaProgression.Instance.AddMetaCurrency(item.metaCurrencyReward);
            }
            
            if (item.skillPointsReward > 0)
            {
                MetaProgression.Instance.AddSkillPoints(item.skillPointsReward);
            }
            
            if (item.experienceReward > 0)
            {
                MetaProgression.Instance.GainExperience(item.experienceReward);
            }
        }
    }
    
    void ShowPickupNotification(LootItem item)
    {
        // Create a simple text notification
        GameObject notification = new GameObject("PickupNotification");
        notification.transform.position = transform.position + Vector3.up * 2f;
        
        // Add text component (you'll need to set up UI properly)
        Debug.Log($"<color={GetRarityColorHex(item.rarity)}>{item.name}</color> acquired!");
    }
    
    string GetRarityColorHex(LootRarity rarity)
    {
        switch (rarity)
        {
            case LootRarity.Common: return "#FFFFFF";
            case LootRarity.Uncommon: return "#00FF00";
            case LootRarity.Rare: return "#0080FF";
            case LootRarity.Epic: return "#FF00FF";
            case LootRarity.Legendary: return "#FFFF00";
            default: return "#FFFFFF";
        }
    }
}
