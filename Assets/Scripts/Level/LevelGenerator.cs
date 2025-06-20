using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] MeshFilter m_MeshFilter;
    [SerializeField] MeshCollider m_Collider;

    private Mesh m_GeneratedMesh;

    void Start()
    {
        MeshFactory.MeshGenerationSettings settings = new
        (
            VertexCountPerSide:     250,
            DistBetweenVerticies:   1
        );

        m_GeneratedMesh = MeshFactory.Create(settings);

        m_MeshFilter.sharedMesh = m_GeneratedMesh;
        m_Collider.sharedMesh = m_GeneratedMesh;
    }
}
