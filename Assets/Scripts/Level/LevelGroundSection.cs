using Unity.AI.Navigation;
using UnityEngine;

public class LevelGroundSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshCollider m_Collider;
    [SerializeField] MeshFilter m_Filter;
    public void SetMesh(Mesh mesh)
    {
        // Assigns the mesh to the collider and filter //
        m_Collider.sharedMesh = mesh;
        m_Filter.sharedMesh = mesh;
    }
}
