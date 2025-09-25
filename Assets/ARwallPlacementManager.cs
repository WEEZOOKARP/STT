using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Debug = UnityEngine.Debug;




// ====== MAIN PLACER ======
[RequireComponent(typeof(ARRaycastManager))]
public class ARWallPlacementManager : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Grid")]
    [SerializeField, Min(0.05f)] private float cellSize = 0.25f;
    [SerializeField] private bool horizontalPlanesOnly = true;

    [Header("Placeables")]
    [SerializeField] private List<WallType> wallTypes = new();
    [SerializeField] private int selectedIndex = 0;

    [Header("Ghost preview")]
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Color validColor = new(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new(1f, 0f, 0f, 0.5f);

    [Header("Aim")]
    [Tooltip("Raycasts from the screen center each frame while preview is enabled.")]
    [SerializeField] private bool aimWithScreenCenter = true;

    // Runtime
    private GameObject ghost;
    private ARPlane currentPlane;
    private Vector3 currentSnapPos;
    private bool currentCanPlace;
    private bool previewEnabled;
    private int rotationSteps;

    private static readonly List<ARRaycastHit> hits = new();
    private readonly Dictionary<TrackableId, HashSet<Vector2Int>> occupied = new();
    private MaterialPropertyBlock mpb;

    // ---------- Lifecycle ----------
    void OnValidate()
    {
        if (!arCamera) arCamera = Camera.main ? Camera.main : GetComponentInChildren<Camera>();
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>() ?? FindObjectOfType<ARRaycastManager>();
        if (!planeManager) planeManager = GetComponent<ARPlaneManager>() ?? FindObjectOfType<ARPlaneManager>();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, wallTypes.Count - 1));
    }
    void Awake() { OnValidate(); mpb = new MaterialPropertyBlock(); }
    void OnEnable() { if (planeManager) planeManager.planesChanged += OnPlanesChanged; }
    void OnDisable() { if (planeManager) planeManager.planesChanged -= OnPlanesChanged; }

    void Update()
    {
        if (!previewEnabled || !raycastManager || !arCamera || wallTypes.Count == 0) return;

        Vector2 screenPos = aimWithScreenCenter
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : (Vector2)Input.mousePosition;

        UpdatePreview(screenPos);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R)) RotateCW();
#endif
    }

    // ---------- UI (wire these) ----------
    public void ShowGhost()
    {
        previewEnabled = true;
        EnsureGhost();
        UpdatePreview(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
    }
    public void HideGhost() { previewEnabled = false; if (ghost) ghost.SetActive(false); }
    public void RotateCW() { rotationSteps = (rotationSteps + 1) & 3; }
    public void RotateCCW() { rotationSteps = (rotationSteps + 3) & 3; }
    public void NextType() { if (wallTypes.Count == 0) return; selectedIndex = (selectedIndex + 1) % wallTypes.Count; RebuildGhost(); }
    public void PrevType() { if (wallTypes.Count == 0) return; selectedIndex = (selectedIndex - 1 + wallTypes.Count) % wallTypes.Count; RebuildGhost(); }
    public void SelectWallType(int index) { if (wallTypes.Count == 0) return; selectedIndex = Mathf.Clamp(index, 0, wallTypes.Count - 1); RebuildGhost(); }

    public void PlaceCurrent()
    {
        if (!previewEnabled || !currentCanPlace || !currentPlane) return;
        var type = CurrentType();
        if (type == null || type.prefab == null) return;

        Vector2Int originCell = GetCurrentCell();
        if (!CanPlaceAt(currentPlane, originCell, type)) return;

        Quaternion rot = ghost ? ghost.transform.rotation : Quaternion.identity;
        Vector3 pos = currentSnapPos + currentPlane.transform.up * type.yOffset;

        GameObject go = Instantiate(type.prefab, pos, rot);

        // Configure according to kind
        switch (type.kind)
        {
            case WallKind.SolidWall:
                if (type.addCarvingObstacle)
                {
                    var obs = go.GetComponent<NavMeshObstacle>() ?? go.AddComponent<NavMeshObstacle>();
                    obs.carving = true;
                    if (obs.shape == NavMeshObstacleShape.Box && obs.size == Vector3.zero)
                        obs.size = new Vector3(1f, 1f, 0.2f); // sane default
                }
                break;

            case WallKind.DamagePole:
                {
                    // ensure there’s a trigger volume
                    var col = EnsurePoleTrigger(go, type.effectRadius);
                    var dz = go.GetComponent<DamageZone>() ?? go.AddComponent<DamageZone>();
                    dz.damageMultiplier = Mathf.Max(1f, type.damageMultiplier);
                }
                break;

            case WallKind.SlowPole:
                {
                    var col = EnsurePoleTrigger(go, type.effectRadius);
                    var sz = go.GetComponent<SlowZone>() ?? go.AddComponent<SlowZone>();
                    sz.speedMultiplier = Mathf.Clamp(type.speedMultiplier, 0.05f, 1f);
                }
                break;
        }

        ClaimCells(currentPlane, originCell, type);
    }

    // Create/size a capsule trigger on the root for poles
    CapsuleCollider EnsurePoleTrigger(GameObject go, float radius)
    {
        var col = go.GetComponent<CapsuleCollider>() ?? go.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.05f, radius);
        col.height = Mathf.Max(2f, radius * 2.2f);
        col.center = Vector3.up * (col.height * 0.5f - 0.1f);
        col.direction = 1; // Y
        // never add NavMeshObstacle on poles (they shouldn't block)
        var obs = go.GetComponent<NavMeshObstacle>();
        if (obs) Destroy(obs);
        return col;
    }

    // ---------- Preview & snapping ----------
    private void UpdatePreview(Vector2 screenPos)
    {
        currentCanPlace = false;

        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            raycastManager.Raycast(screenPos, hits, TrackableType.Planes);

        if (hits.Count == 0)
        {
            if (ghost) ghost.SetActive(false);
            currentPlane = null;
            return;
        }

        var hit = hits[0];
        currentPlane = hit.trackable as ARPlane;
        if (!currentPlane) { if (ghost) ghost.SetActive(false); return; }
        if (horizontalPlanesOnly && currentPlane.alignment != PlaneAlignment.HorizontalUp) { if (ghost) ghost.SetActive(false); return; }

        var type = CurrentType();
        if (type == null || type.prefab == null) { if (ghost) ghost.SetActive(false); return; }

        EnsureGhost();

        Vector3 local = currentPlane.transform.InverseTransformPoint(hit.pose.position);
        int cx = Mathf.RoundToInt(local.x / cellSize);
        int cz = Mathf.RoundToInt(local.z / cellSize);
        Vector3 localSnapped = new Vector3(cx * cellSize, 0f, cz * cellSize);
        currentSnapPos = currentPlane.transform.TransformPoint(localSnapped);

        Vector3 forward = Vector3.ProjectOnPlane(arCamera.transform.forward, currentPlane.transform.up).normalized;
        if (forward.sqrMagnitude < 1e-4f) forward = currentPlane.transform.forward;
        Quaternion baseRot = Quaternion.LookRotation(forward, currentPlane.transform.up);
        Quaternion addRot = Quaternion.AngleAxis(90f * rotationSteps, currentPlane.transform.up);

        ghost.transform.SetPositionAndRotation(
            currentSnapPos + currentPlane.transform.up * type.yOffset,
            addRot * baseRot
        );

        Vector2Int origin = new Vector2Int(cx, cz);
        currentCanPlace = CanPlaceAt(currentPlane, origin, type);
        TintGhost(currentCanPlace ? validColor : invalidColor);
        ghost.SetActive(true);
    }

    // ---------- Helpers ----------
    private WallType CurrentType()
        => (wallTypes == null || wallTypes.Count == 0) ? null : wallTypes[Mathf.Clamp(selectedIndex, 0, wallTypes.Count - 1)];

    private Vector2Int GetCurrentCell()
    {
        Vector3 local = currentPlane.transform.InverseTransformPoint(currentSnapPos);
        return new Vector2Int(
            Mathf.RoundToInt(local.x / cellSize),
            Mathf.RoundToInt(local.z / cellSize)
        );
    }

    private bool CanPlaceAt(ARPlane plane, Vector2Int origin, WallType type)
    {
        var set = GetPlaneSet(plane.trackableId);
        foreach (var c in Footprint(origin, type.sizeInCells, rotationSteps))
            if (set.Contains(c)) return false;
        return true;
    }

    private void ClaimCells(ARPlane plane, Vector2Int origin, WallType type)
    {
        var set = GetPlaneSet(plane.trackableId);
        foreach (var c in Footprint(origin, type.sizeInCells, rotationSteps))
            set.Add(c);
    }

    private HashSet<Vector2Int> GetPlaneSet(TrackableId id)
    {
        if (!occupied.TryGetValue(id, out var set))
            occupied[id] = set = new HashSet<Vector2Int>();
        return set;
    }

    private IEnumerable<Vector2Int> Footprint(Vector2Int origin, Vector2Int size, int rotSteps)
    {
        Vector2Int s = ((rotSteps & 1) == 0) ? size : new Vector2Int(size.y, size.x);
        for (int x = 0; x < s.x; x++)
            for (int z = 0; z < s.y; z++)
                yield return new Vector2Int(origin.x + x, origin.y + z);
    }

    private void EnsureGhost()
    {
        var type = CurrentType();
        if (ghost != null && type != null && ghost.name.Contains(type.name)) return;
        RebuildGhost();
    }

    private void RebuildGhost()
    {
        if (ghost) Destroy(ghost);

        var type = CurrentType();
        if (type == null || type.prefab == null)
        {
            Debug.LogError("ARWallPlacementManager: Missing WallType or Prefab.");
            return;
        }

        ghost = Instantiate(type.prefab);
        ghost.name = $"Ghost_{type.name}";

        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        if (ghostMaterial)
            foreach (var r in ghost.GetComponentsInChildren<Renderer>()) r.sharedMaterial = ghostMaterial;

        ghost.SetActive(previewEnabled);
    }

    private void TintGhost(Color c)
    {
        if (!ghost) return;
        mpb ??= new MaterialPropertyBlock();

        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
        {
            if (r.sharedMaterial && r.sharedMaterial.HasProperty("_BaseColor"))
                mpb.SetColor("_BaseColor", c);
            else
                mpb.SetColor("_Color", c);

            r.SetPropertyBlock(mpb);
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs evt)
    {
        foreach (var p in evt.removed)
            occupied.Remove(p.trackableId);
    }
}

// ====== Minimal zone components (same file) ======
public class DamageZone : MonoBehaviour
{
    public float damageMultiplier = 1.5f;

    void OnTriggerEnter(Collider other)
    {
        var e = other.GetComponent<EnemyBehavior>();
        if (e) e.ApplyDamageMultiplier(damageMultiplier);
    }
    void OnTriggerExit(Collider other)
    {
        var e = other.GetComponent<EnemyBehavior>();
        if (e) e.ResetDamageMultiplier();
    }
}

public class SlowZone : MonoBehaviour
{
    [Range(0.05f, 1f)] public float speedMultiplier = 0.6f;

    void OnTriggerEnter(Collider other)
    {
        var e = other.GetComponent<EnemyBehavior>();
        if (e) e.ApplySpeedMultiplier(speedMultiplier);
    }
    void OnTriggerExit(Collider other)
    {
        var e = other.GetComponent<EnemyBehavior>();
        if (e) e.ResetSpeedMultiplier();
    }
}
// ===== Simple data for each placeable =====
public enum WallKind { SolidWall, DamagePole, SlowPole }

[System.Serializable]
public class WallType   // <-- NOT MonoBehaviour/ScriptableObject
{
    public string name = "Piece";
    public GameObject prefab;
    public Vector2Int sizeInCells = new Vector2Int(1, 1);
    public float yOffset = 0f;
    public bool addCarvingObstacle = false;
    public WallKind kind = WallKind.SolidWall;
    public float effectRadius = 1.5f;
    public float damageMultiplier = 1.5f;
    public float speedMultiplier = 0.6f;
}