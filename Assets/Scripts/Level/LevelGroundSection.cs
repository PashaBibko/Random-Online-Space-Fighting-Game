using Unity.AI.Navigation;
using UnityEngine;

public class LevelGroundSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshCollider m_Collider;
    [SerializeField] MeshFilter m_Filter;
    [SerializeField] NavMeshSurface m_Surface;

    public void SetMesh(Mesh mesh)
    {
        // Assigns the mesh to the collider and filter //
        m_Collider.sharedMesh = mesh;
        m_Filter.sharedMesh = mesh;

        // Bakes the nav mesh so the pathfinding can use the surface //
        m_Surface.collectObjects = CollectObjects.All;
        m_Surface.BuildNavMesh();
    }
}
