using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public abstract class HeightMapFunction : ScriptableObject
{
    public abstract float SampleHeight(Vector2 location, int seed);
}

public partial class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Material m_ChunkRenderMaterial;
    [SerializeField] ComputeShader m_HeightmapComputeShader;

    [Header("Controls")]
    [SerializeField] bool m_RandomiseWorldSeed;
    [SerializeField] bool m_GenerateValley;

    [Header("Generation Settings")]
    [SerializeField] uint m_ChunkSampleCount;
    [SerializeField] uint m_ChunkSize;
    [SerializeField] Vector2Int m_ChunkCount;
    [SerializeField] uint m_WorldSeed;
    [SerializeField] uint m_WorldScale;

    float[] m_Heightmap;

    int width = -1;
    int height = -1;

    private void Start()
    {
        #if UNITY_EDITOR
            // Gathers all previously generated children //
            List<GameObject> children = new();
            foreach (Transform child in transform) { children.Add(child.gameObject); }

            // If there are no children it needs to generate a world //
            if (children.Count == 0) { Generate(); }
        #else
            // Does not need to do any checks in release builds //
            Generate();
        #endif
    }

    public void Generate()
    {
        // Randomises the world seed //
        if (m_RandomiseWorldSeed) { m_WorldSeed = (uint)Random.Range(1, 10000); }

        if (m_GenerateValley)
        {
            // Generates the valley of the world //
            ValleyNode valley = new ValleyNode(m_WorldSeed);
            valley.Shift(out Vector2 size);

            m_ChunkCount.x = (int)size.x;
            m_ChunkCount.y = (int)size.y;
        }

        // Allocates the memory for the heightmap //
        m_Heightmap = new float[(m_ChunkCount.x * m_ChunkSampleCount + 1) * (m_ChunkCount.y * m_ChunkSampleCount + 1)];
        width = (int)(m_ChunkCount.x * m_ChunkSampleCount + 1);
        height = (int)(m_ChunkCount.y * m_ChunkSampleCount + 1);

        // Sets shader constants //
        m_HeightmapComputeShader.SetInt("width", width);
        m_HeightmapComputeShader.SetInt("height", height);

        m_HeightmapComputeShader.SetInt("seed", (int)m_WorldSeed);
        m_HeightmapComputeShader.SetInt("scale", (int)m_WorldScale);

        m_HeightmapComputeShader.SetInt("octaves", 8);
        m_HeightmapComputeShader.SetFloat("persistence", 0.4f);
        m_HeightmapComputeShader.SetFloat("lacunarity", 2.0f);

        // Runs the compute shader to generate the heightmap //
        Vector2Int kernelSize = new
        (
            Mathf.CeilToInt(width / 8f),
            Mathf.CeilToInt(height / 8f)
        );
        ComputeBuffer buffer = new(m_Heightmap.Length, sizeof(float));
        int kernelIndex = m_HeightmapComputeShader.FindKernel("CSMain");
        m_HeightmapComputeShader.SetBuffer(kernelIndex, "ResultBuffer", buffer);
        m_HeightmapComputeShader.Dispatch(kernelIndex, kernelSize.x, kernelSize.y, 1);

        // Copies the data from the buffer to the heightmap and then frees the buffer //
        buffer.GetData(m_Heightmap);
        buffer.Release();

        // Generates the worlds chunks //
        for (int x = 0; x < m_ChunkCount.x; x++)
        {
            for (int z = 0; z < m_ChunkCount.y; z++)
            {
                // Creates a gameobject to hold the needed info //
                GameObject chunk = new("WorldChunk");
                chunk.transform.SetParent(transform, false);

                // Places it in the world correctly //
                chunk.transform.position = new Vector3(x * m_ChunkSize, 0, z * m_ChunkSize);

                // Adds the components of the world chunk //
                MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();
                MeshFilter filter = chunk.AddComponent<MeshFilter>();

                // Generates the mesh and assigns it //
                Mesh meshSect = GenerateMeshSection(new Vector2(x, z));
                filter.sharedMesh = meshSect;

                // Assigns a material so it doesn't render as pink //
                renderer.material = m_ChunkRenderMaterial;
            }
        }
    }
}
