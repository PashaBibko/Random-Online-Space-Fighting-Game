using System.Collections.Generic;
using System.Linq.Expressions;
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
    [SerializeField] ComputeShader m_ValleyWeightComputeShader;

    [Header("Controls")]
    [SerializeField] bool m_RandomiseWorldSeed;
    [SerializeField] bool m_GenerateValley;

    [Header("Generation Settings")]
    [SerializeField] uint m_ChunkSampleCount;
    [SerializeField] uint m_ChunkSize;
    [SerializeField] Vector2Int m_ChunkCount;
    [SerializeField] uint m_WorldSeed;
    [SerializeField] uint m_WorldScale;

    ValleyNode m_Valley;

    float[] m_Heightmap;
    float[] m_Weightmap;

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

        // Generates the valley of the world section //
        if (m_GenerateValley)
        {
            // Generates the valley of the world //
            m_Valley = new ValleyNode(m_WorldSeed);
            m_Valley.Shift(out Vector2 size);

            m_ChunkCount.x = (int)size.x;
            m_ChunkCount.y = (int)size.y;
        }

        // Allocates the memory for the heightmap and weightmap //
        width = (int)(m_ChunkCount.x * m_ChunkSampleCount + 1);
        height = (int)(m_ChunkCount.y * m_ChunkSampleCount + 1);
        m_Heightmap = new float[width * height];
        m_Weightmap = new float[width * height];

        // Fills the weightmap (if valley generation enabled) //
        if (m_GenerateValley)
        {
            int kernelIndex0 = m_ValleyWeightComputeShader.FindKernel("CSMain");

            // Gets an array of all of the connections and passes it to the shader //
            Vector2[] valleyConnections = m_Valley.SerializeConnections(m_ChunkSampleCount);

            ComputeBuffer connectionBuffer = new(valleyConnections.Length, sizeof(float) * 2);
            connectionBuffer.SetData(valleyConnections);

            m_ValleyWeightComputeShader.SetBuffer(kernelIndex0, "ValleyConnections", connectionBuffer);
            m_ValleyWeightComputeShader.SetInt("ValleyConnectionsCount", (int)(valleyConnections.Length / 2f));

            // Sets the size of the grid in the shader //
            m_ValleyWeightComputeShader.SetInt("width", width);
            m_ValleyWeightComputeShader.SetInt("height", height);

            // Sets the output buffer and then dispatches the shader //
            ComputeBuffer resultBuffer = new(m_Weightmap.Length, sizeof(float));
            m_ValleyWeightComputeShader.SetBuffer(kernelIndex0, "ResultBuffer", resultBuffer);

            Vector2Int kernelSize0 = new
            (
                Mathf.CeilToInt(width / 8f),
                Mathf.CeilToInt(height / 8f)
            );
            m_ValleyWeightComputeShader.Dispatch(kernelIndex0, kernelSize0.x, kernelSize0.y, 1);

            // Copies the data and frees the buffer //
            resultBuffer.GetData(m_Weightmap);
            resultBuffer.Release();
        }

        // Sets shader constants //
        m_HeightmapComputeShader.SetInt("width", width);
        m_HeightmapComputeShader.SetInt("height", height);

        m_HeightmapComputeShader.SetInt("seed", (int)m_WorldSeed);
        m_HeightmapComputeShader.SetInt("scale", (int)m_WorldScale);

        m_HeightmapComputeShader.SetInt("octaves", 8);
        m_HeightmapComputeShader.SetFloat("persistence", 0.4f);
        m_HeightmapComputeShader.SetFloat("lacunarity", 2.0f);

        // Runs the compute shader to generate the heightmap //
        Vector2Int kernelSize1 = new
        (
            Mathf.CeilToInt(width / 8f),
            Mathf.CeilToInt(height / 8f)
        );
        ComputeBuffer buffer = new(m_Heightmap.Length, sizeof(float));
        int kernelIndex1 = m_HeightmapComputeShader.FindKernel("CSMain");
        m_HeightmapComputeShader.SetBuffer(kernelIndex1, "ResultBuffer", buffer);
        m_HeightmapComputeShader.Dispatch(kernelIndex1, kernelSize1.x, kernelSize1.y, 1);

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
