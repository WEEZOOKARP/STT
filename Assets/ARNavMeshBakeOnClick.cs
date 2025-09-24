using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.AI.Navigation;
// Make sure "Debug" means Unity's logger (avoids clash with System.Diagnostics.Debug)
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(ARPlaneManager))]
public class ARNavMeshBakeOnClick : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public NavMeshSurface navSurface;   // Must collect ARPlane layer, Use Geometry = Render Meshes
    public Transform follow;            // XR Origin or Main Camera (keeps bounds over scanned area)

    [Header("Bounds used when you press Bake")]
    public Vector3 boundsSize = new Vector3(50f, 10f, 50f); // X/Z = width/depth (m), Y = height (m)
    public bool recenterBoundsEachBake = true;              // center the surface over 'follow' before baking

    ARPlaneManager planeManager;

    void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
        if (!follow && Camera.main) follow = Camera.main.transform;
        if (!navSurface) Debug.LogWarning("[NavBake] NavMeshSurface not assigned.");
    }

    // Hook this to your UI Button's OnClick
    public void BakeNow()
    {
        if (!navSurface)
        {
            Debug.LogWarning("[NavBake] No NavMeshSurface assigned.");
            return;
        }

        // Keep the collection box over the scanned area
        if (recenterBoundsEachBake && follow)
        {
            navSurface.center = navSurface.transform.InverseTransformPoint(follow.position);
            navSurface.size = boundsSize;
        }

        // Build!
        navSurface.RemoveData();
        navSurface.BuildNavMesh(); // returns void in Unity 6 AI Navigation
        Debug.Log($"[NavBake] Built. Center={navSurface.center} Size={navSurface.size}");
    }

    // Optional convenience if you want a right-click context menu in the Inspector
    [ContextMenu("Bake Now (Context Menu)")]
    void BakeNowContextMenu() => BakeNow();
}
