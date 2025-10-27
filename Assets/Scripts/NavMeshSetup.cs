using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshSetup : MonoBehaviour
{
    [Header("NavMesh Configuration")]
    public NavMeshSurface navMeshSurface;
    public Transform groundPlane;
    
    void Start()
    {
        SetupNavMesh();
    }
    
    void SetupNavMesh()
    {
        // Create NavMeshSurface if it doesn't exist
        if (navMeshSurface == null)
        {
            GameObject navMeshObj = new GameObject("NavMeshSurface");
            navMeshSurface = navMeshObj.AddComponent<NavMeshSurface>();
        }
        
        // Configure NavMeshSurface
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.layerMask = -1; // All layers
        
        // Build the NavMesh
        navMeshSurface.BuildNavMesh();
        
        Debug.Log("NavMesh built successfully!");
    }
    
    // Call this method to rebuild NavMesh if needed
    [ContextMenu("Rebuild NavMesh")]
    public void RebuildNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh rebuilt!");
        }
    }
}
