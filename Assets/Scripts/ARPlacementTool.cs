using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.AI.Navigation;

public class ARPlacementTool : MonoBehaviour
{
    [Header("AR")]
    public Camera arCamera;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Ghost look (optional)")]
    public Material ghostMaterial;
    public Color valid = new(0, 1, 0, 0.5f), invalid = new(1, 0, 0, 0.5f);

    [Header("Placeable Prefabs")]
    public GameObject strongholdPrefab; // Tag this prefab "Base"
    public GameObject spawnerPrefab;    // Add SpawnerMarker to it

    GameObject ghost, currentPrefab;
    static readonly List<ARRaycastHit> hits = new();
    MaterialPropertyBlock mpb;

    void Awake()
    {
        if (!arCamera && Camera.main) arCamera = Camera.main;
        if (!raycastManager) raycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (!planeManager) planeManager = FindFirstObjectByType<ARPlaneManager>();
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (!currentPrefab) return;

        Vector2 center = new(Screen.width * .5f, Screen.height * .5f);
        if (!raycastManager.Raycast(center, hits, TrackableType.PlaneWithinPolygon))
        { if (ghost) ghost.SetActive(false); return; }

        var hit = hits[0];
        var pose = hit.pose;

        EnsureGhost();
        ghost.transform.SetPositionAndRotation(pose.position, pose.rotation);
        bool isHorizontal = !(hit.trackable is ARPlane p) || p.alignment == PlaneAlignment.HorizontalUp;
        TintGhost(isHorizontal ? valid : invalid);
        ghost.SetActive(true);

#if UNITY_EDITOR
        bool tapped = Input.GetMouseButtonDown(0);
#else
        bool tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif
        if (tapped && isHorizontal) PlaceNow(pose.position, pose.rotation);
    }

    void EnsureGhost()
    {
        if (ghost && ghost.name.Contains(currentPrefab.name)) return;
        if (ghost) Destroy(ghost);
        ghost = Instantiate(currentPrefab);
        ghost.name = $"Ghost_{currentPrefab.name}";
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
        if (ghostMaterial)
            foreach (var r in ghost.GetComponentsInChildren<Renderer>()) r.sharedMaterial = ghostMaterial;
    }
    void TintGhost(Color c)
    {
        if (!ghost) return;
        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
        {
            if (r.sharedMaterial && r.sharedMaterial.HasProperty("_BaseColor"))
                mpb.SetColor("_BaseColor", c);
            else mpb.SetColor("_Color", c);
            r.SetPropertyBlock(mpb);
        }
    }
    void PlaceNow(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(currentPrefab, pos, rot);

        // Help pathfinding: carve around placed objects
        var obs = go.GetComponent<UnityEngine.AI.NavMeshObstacle>() ?? go.AddComponent<UnityEngine.AI.NavMeshObstacle>();
        obs.carving = true;

        ghost?.SetActive(false);
        currentPrefab = null;
    }

    // UI hooks
    public void BeginPlaceStronghold() { currentPrefab = strongholdPrefab; }
    public void BeginPlaceSpawner() { currentPrefab = spawnerPrefab; }
    public void CancelPlacement() { currentPrefab = null; ghost?.SetActive(false); }
}
