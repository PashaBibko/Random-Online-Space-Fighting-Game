using System.Collections.Generic;
using UnityEngine;

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

    private void GenerateValley()
    {
        // Generates the valley of the world //
        m_Valley = new ValleyNode(m_WorldSeed);
        m_Valley.Shift(out Vector2 size);

        m_ChunkCount.x = (int)size.x;
        m_ChunkCount.y = (int)size.y;
    }

    private void GenerateBasicHeightmap()
    {
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
    }

    private void GenerateValleyWeightmap()
    {
        int kernelIndex = m_ValleyWeightComputeShader.FindKernel("CSMain");

        // Gets an array of all of the connections and passes it to the shader //
        Vector2[] valleyConnections = m_Valley.SerializeConnections(m_ChunkSampleCount);

        ComputeBuffer connectionBuffer = new(valleyConnections.Length, sizeof(float) * 2);
        connectionBuffer.SetData(valleyConnections);

        m_ValleyWeightComputeShader.SetBuffer(kernelIndex, "ValleyConnections", connectionBuffer);
        m_ValleyWeightComputeShader.SetInt("ValleyConnectionsCount", (int)(valleyConnections.Length / 2f));

        // Sets the size of the grid in the shader //
        m_ValleyWeightComputeShader.SetInt("width", width);
        m_ValleyWeightComputeShader.SetInt("height", height);

        // Sets the output buffer and then dispatches the shader //
        ComputeBuffer resultBuffer = new(m_Weightmap.Length, sizeof(float));
        m_ValleyWeightComputeShader.SetBuffer(kernelIndex, "ResultBuffer", resultBuffer);

        Vector2Int kernelSize = new
        (
            Mathf.CeilToInt(width / 8f),
            Mathf.CeilToInt(height / 8f)
        );
        m_ValleyWeightComputeShader.Dispatch(kernelIndex, kernelSize.x, kernelSize.y, 1);

        // Copies the data and frees the buffer //
        resultBuffer.GetData(m_Weightmap);
        resultBuffer.Release();
    }

    private void ApplyWeightmapToHeightmap()
    {
        // Loop over the entire heightmap //
        for (int i = 0; i < m_Heightmap.Length; i++)
        {
            // Translates the weight to the bounds of [400, 1000] //
            float weight = Mathf.Max(0.2f, Mathf.Clamp01(m_Weightmap[i] / 200f) / 2f) * 2000f;
            m_Heightmap[i] *= weight; // Applies it to the heightmap //
        }
    }

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

    public void Generate()
    {
        // Randomises the world seed //
        if (m_RandomiseWorldSeed) { m_WorldSeed = (uint)Random.Range(1, 10000); }

        // Generates the valley of the world section //
        if (m_GenerateValley) { GenerateValley(); }

        // Allocates the memory for the heightmap and weightmap //
        width = (int)(m_ChunkCount.x * m_ChunkSampleCount + 1);
        height = (int)(m_ChunkCount.y * m_ChunkSampleCount + 1);
        m_Heightmap = new float[width * height];
        m_Weightmap = new float[width * height];

        // Generates the basic heightmap //
        GenerateBasicHeightmap();

        // Fills the weightmap (if valley generation enabled) //
        if (m_GenerateValley)
        {
            GenerateValleyWeightmap();
            ApplyWeightmapToHeightmap();
        }

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
