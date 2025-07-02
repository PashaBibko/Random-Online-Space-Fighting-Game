using UnityEngine;

public partial class TerrainGenerator : MonoBehaviour
{
    private Mesh GenerateMeshSection(Vector2 location)
    {
        // Trackers for location in the arrays //
        int vIndex = 0;
        int tIndex = 0;

        // Calculates the size of the arrays //
        int vertexCount = (int)Mathf.Pow(m_ChunkSampleCount + 1, 2);
        int triangleCount = (int)Mathf.Pow(m_ChunkSampleCount, 2) * 6;

        // Allocates space for the arrays //
        Vector3[] vertecies = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];

        // The offset of the gameobject in world space //
        Vector2 worldOffset = location * m_ChunkSize;

        // Generates the vertex positions //
        for (int z = 0; z < m_ChunkSampleCount + 1; z++)
        {
            for (int x = 0; x < m_ChunkSampleCount + 1; x++)
            {
                // Calculates the world position relative to the mesh gameobject //
                Vector2 pos = new Vector2(x, z) / m_ChunkSampleCount * m_ChunkSize;

                // Samples the heightmap at the given location //
                Vector2 samplePos = (location * m_ChunkSampleCount) + new Vector2(x, z);
                float sample = m_Heightmap[((int)samplePos.y * width) + (int)samplePos.x];

                // Assigns it to the vertex position //
                vertecies[vIndex] = new Vector3(pos.x, sample, pos.y);
                vIndex++;
            }
        }

        // Generates the triangles from the vertecies //
        for (int z = 0; z < m_ChunkSampleCount; z++)
        {
            for (int x = 0; x < m_ChunkSampleCount; x++)
            {
                // The general offset within the verticies array //
                int offset = z * ((int)m_ChunkSampleCount + 1) + x;

                // First triangle of the quad //
                triangles[tIndex + 0] = offset + 0;
                triangles[tIndex + 1] = offset + 1 + (int)m_ChunkSampleCount;
                triangles[tIndex + 2] = offset + 1;

                // Second triangle of the quad //
                triangles[tIndex + 3] = offset + 1;
                triangles[tIndex + 4] = offset + 1 + (int)m_ChunkSampleCount;
                triangles[tIndex + 5] = offset + 2 + (int)m_ChunkSampleCount;

                // Iterates to the next triangle //
                tIndex += 6;
            }
        }

        // Assigns data to a mesh and returns it //
        Mesh mesh = new()
        {
            vertices = vertecies,
            triangles = triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
