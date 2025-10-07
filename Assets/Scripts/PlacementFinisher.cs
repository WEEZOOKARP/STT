// PlacementFinisher.cs

using System.Linq;
using UnityEngine;

public class PlacementFinisher : MonoBehaviour
{
    [SerializeField] private GameObject placementPanel;   // assign in Inspector

    public void ApplyAndStart()
    {
        var wm = FindObjectOfType<WaveManager>();
        if (!wm) { Debug.LogError("[PlacementFinisher] No WaveManager found."); return; }

        // Collect placed spawners (needs SpawnerMarker on the spawner prefab)
        wm.spawnPoints = FindObjectsOfType<SpawnerMarker>()
                         .Select(m => m.transform)
                         .ToArray();
        Debug.Log($"[PlacementFinisher] Spawn points set = {wm.spawnPoints.Length}");

        // Find stronghold by Tag = Base
        Transform stronghold = null;
        var shGo = GameObject.FindGameObjectWithTag("Base");
        if (shGo) stronghold = shGo.transform;
        if (!stronghold) Debug.LogWarning("[PlacementFinisher] No stronghold tagged 'Base' found.");

        wm.strongholdTarget = stronghold;

        // Start waves now (begin from wave 1)
        wm.BeginRun(1);

        if (placementPanel) placementPanel.SetActive(false);
    }
}
