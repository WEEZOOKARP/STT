using UnityEngine;

[CreateAssetMenu(menuName = "AR/Grid/Wall Type")]
public class WallType : ScriptableObject
{
    [Tooltip("Prefab to spawn (include your visuals & effect scripts here).")]
    public GameObject prefab;

    [Tooltip("Size in grid cells (width, depth). 1,1 = one cell.")]
    public Vector2Int sizeInCells = new(1, 1);

    [Tooltip("Add/ensure NavMeshObstacle(carving) on spawn.")]
    public bool addCarvingObstacle = true;

    [Tooltip("Optional: how high to place base above plane (meters).")]
    public float yOffset = 0.0f;
}
