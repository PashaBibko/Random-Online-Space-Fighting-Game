using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HeightMapFunction : ScriptableObject
{
    public abstract float SampleHeight(Vector2 location, int seed);
}

public partial class TerrainGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Material m_ChunkRenderMaterial;
    [SerializeField] ComputeWrapper m_HeightmapComputeShader;

    [Header("Controls")]
    [SerializeField] bool m_RandomiseWorldSeed;
    [SerializeField] bool runErosionSimulation;
    [SerializeField] bool m_EnableSmoothing;

    [Header("Generation Settings")]
    [SerializeField] uint m_ChunkSampleCount;
    [SerializeField] uint m_ChunkSize;
    [SerializeField] Vector2Int m_ChunkCount;

    [Header("Noise settings")]
    [SerializeField] uint m_WorldSeed;
    [SerializeField] uint m_WorldScale;

    [Header("Erosion settings")]
    [Range(2, 8), SerializeField] int erosionRadius;
    [Range(0, 1), SerializeField] float inertia; 
    [SerializeField] float sedimentCapacityFactor;
    [SerializeField] float minSedimentCapacity;
    [Range(0, 1), SerializeField] float erodeSpeed;
    [Range(0, 1), SerializeField] float depositSpeed;
    [Range(0, 1), SerializeField] float evaporateSpeed;
    [SerializeField] float gravity;
    [SerializeField] int maxDropletLifetime;
    [SerializeField] float initialWaterVolume;
    [SerializeField] float initialSpeed;
    [SerializeField] int m_WaterDropsPerSample;

    float[] m_Heightmap;

    int width = -1;
    int height = -1;

    // Indices and weights of erosion brush precomputed for every node //
    int[][] erosionBrushIndices;
    float[][] erosionBrushWeights;
    System.Random prng;

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

        // Allocates the memory for the heightmap //
        m_Heightmap = new float[(m_ChunkCount.x * m_ChunkSampleCount + 1) * (m_ChunkCount.y * m_ChunkSampleCount + 1)];
        width = (int)(m_ChunkCount.x * m_ChunkSampleCount + 1);
        height = (int)(m_ChunkCount.y * m_ChunkSampleCount + 1);

        // Runs the compute shader to generate the heightmap //
        Vector2Int kernelSize = Vector2Int.one;
        m_HeightmapComputeShader.LaunchShader("CS_Main_8", kernelSize, ref m_Heightmap, sizeof(float));

        // Runs the erosion (if it is referenced) //
        if (runErosionSimulation)
        {
            Erode(m_Heightmap, new Vector2Int(width, height), m_Heightmap.Length * m_WaterDropsPerSample);
        }

        if (m_EnableSmoothing)
        {
            // Smooths the heightmap as erosion can make it a bit too bumpy //
            float[] temp = new float[width * height];
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    float total = m_Heightmap[(y * width) + x];

                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            total += m_Heightmap[((y + ny) * width) + (x + nx)];
                        }
                    }

                    temp[(y * width) + x] = total / 9f;
                }
            }

            m_Heightmap = temp;
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
