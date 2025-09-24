
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AI;               
using Unity.AI.Navigation;          
using Debug = UnityEngine.Debug;

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

    [Header("Walls")]
    [SerializeField] private List<WallType> wallTypes = new();
    [SerializeField] private int selectedIndex = 0;

    [Header("Ghost preview")]
    [SerializeField] private Material ghostMaterial;             
    [SerializeField] private Color validColor = new(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new(1f, 0f, 0f, 0.5f);

    [Header("Aim")]
    [Tooltip("Raycasts from the screen center each frame while preview is enabled.")]
    [SerializeField] private bool aimWithScreenCenter = true;

    // Runtime state
    private GameObject ghost;
    private ARPlane currentPlane;
    private Vector3 currentSnapPos;
    private bool currentCanPlace;
    private bool previewEnabled;
    private int rotationSteps; // 0/1/2/3 -> 0/90/180/270 deg

    private static readonly List<ARRaycastHit> hits = new();

    // Per-plane occupied grid cells (prevents overlaps)
    private readonly Dictionary<TrackableId, HashSet<Vector2Int>> occupied = new();

    // Create in Awake (NOT as a field initializer)
    private MaterialPropertyBlock mpb;

    // ---------- Lifecycle ----------
    void OnValidate()
    {
        if (!arCamera) arCamera = Camera.main ? Camera.main : GetComponentInChildren<Camera>();
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>() ?? FindObjectOfType<ARRaycastManager>();
        if (!planeManager) planeManager = GetComponent<ARPlaneManager>() ?? FindObjectOfType<ARPlaneManager>();
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, wallTypes.Count - 1));
    }

    void Awake()
    {
        OnValidate();
        if (mpb == null) mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        if (planeManager) planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        if (planeManager) planeManager.planesChanged -= OnPlanesChanged;
    }

    void Update()
    {
        if (!previewEnabled || !raycastManager || !arCamera || wallTypes.Count == 0) return;

        Vector2 screenPos = aimWithScreenCenter
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : (Vector2)Input.mousePosition;

        UpdatePreview(screenPos);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R)) RotateCW(); // editor helper
#endif
    }

    // ---------- UI (wire these to buttons) ----------
    public void ShowGhost()
    {
        previewEnabled = true;
        EnsureGhost();
        UpdatePreview(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
    }

    public void HideGhost()
    {
        previewEnabled = false;
        if (ghost) ghost.SetActive(false);
    }

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

        if (type.addCarvingObstacle)
        {
            var obs = go.GetComponent<NavMeshObstacle>() ?? go.AddComponent<NavMeshObstacle>();
            obs.carving = true;
            if (obs.shape == NavMeshObstacleShape.Box && obs.size == Vector3.zero)
                obs.size = new Vector3(0.2f, 1.0f, 1.0f); // tweak to your prefab footprint
        }

        ClaimCells(currentPlane, originCell, type);
    }

    public void RotateCW() { rotationSteps = (rotationSteps + 1) & 3; }
    public void RotateCCW() { rotationSteps = (rotationSteps + 3) & 3; }

    public void NextType()
    {
        if (wallTypes == null || wallTypes.Count == 0) return;
        selectedIndex = (selectedIndex + 1) % wallTypes.Count;
        RebuildGhost();
    }

    public void PrevType()
    {
        if (wallTypes == null || wallTypes.Count == 0) return;
        selectedIndex = (selectedIndex - 1 + wallTypes.Count) % wallTypes.Count;
        RebuildGhost();
    }

    public void SelectWallType(int index)
    {
        if (wallTypes == null || wallTypes.Count == 0) return;
        selectedIndex = Mathf.Clamp(index, 0, wallTypes.Count - 1);
        RebuildGhost();
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
        if (!currentPlane)
        {
            if (ghost) ghost.SetActive(false);
            return;
        }

        if (horizontalPlanesOnly && currentPlane.alignment != PlaneAlignment.HorizontalUp)
        {
            if (ghost) ghost.SetActive(false);
            return;
        }

        var type = CurrentType();
        if (type == null || type.prefab == null)
        {
            Debug.LogError("[Preview] WallType or Prefab missing.");
            if (ghost) ghost.SetActive(false);
            return;
        }

        EnsureGhost();

        // Snap to grid (plane local space)
        Vector3 local = currentPlane.transform.InverseTransformPoint(hit.pose.position);
        int cx = Mathf.RoundToInt(local.x / cellSize);
        int cz = Mathf.RoundToInt(local.z / cellSize);
        Vector3 localSnapped = new Vector3(cx * cellSize, 0f, cz * cellSize);
        currentSnapPos = currentPlane.transform.TransformPoint(localSnapped);

        // Orientation: plane aligned + 90° steps
        Vector3 forward = Vector3.ProjectOnPlane(arCamera.transform.forward, currentPlane.transform.up).normalized;
        if (forward.sqrMagnitude < 1e-4f) forward = currentPlane.transform.forward;
        Quaternion baseRot = Quaternion.LookRotation(forward, currentPlane.transform.up);
        Quaternion addRot = Quaternion.AngleAxis(90f * rotationSteps, currentPlane.transform.up);

        ghost.transform.SetPositionAndRotation(
            currentSnapPos + currentPlane.transform.up * type.yOffset,
            addRot * baseRot
        );

        // Validity + tint
        Vector2Int origin = new Vector2Int(cx, cz);
        currentCanPlace = CanPlaceAt(currentPlane, origin, type);
        TintGhost(currentCanPlace ? validColor : invalidColor);
        ghost.SetActive(true);
    }

    // ---------- Helpers ----------
    private WallType CurrentType()
        => (wallTypes == null || wallTypes.Count == 0)
            ? null
            : wallTypes[Mathf.Clamp(selectedIndex, 0, wallTypes.Count - 1)];

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

        // Disable collisions & scripts on the preview
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;

        // Apply ghost material if provided
        if (ghostMaterial)
            foreach (var r in ghost.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = ghostMaterial;

        ghost.SetActive(previewEnabled);
    }

    private void TintGhost(Color c)
    {
        if (!ghost) return;
        if (mpb == null) mpb = new MaterialPropertyBlock(); // safety

        foreach (var r in ghost.GetComponentsInChildren<Renderer>())
        {
            // URP Unlit uses _BaseColor; many shaders accept _Color fallback
            if (r.sharedMaterial && r.sharedMaterial.HasProperty("_BaseColor"))
                mpb.SetColor("_BaseColor", c);
            else
                mpb.SetColor("_Color", c);

            r.SetPropertyBlock(mpb);
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs evt)
    {
        // Clear reservations for removed planes so memory & state stay clean
        foreach (var p in evt.removed)
            occupied.Remove(p.trackableId);
    }
}
