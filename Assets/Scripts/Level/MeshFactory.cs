using System;
using UnityEngine;

public static class MeshFactory
{
    public struct MeshGenerationSettings
    {
        // == General settings == //

        public int _VertexCountPerSide;
        public int _DistBetweenVertecies;

        // == Noise settings == //

        // == Constructor == //
        public MeshGenerationSettings(int VertexCountPerSide, int DistBetweenVerticies)
        {
            _VertexCountPerSide = VertexCountPerSide;
            _DistBetweenVertecies = DistBetweenVerticies;
        }
    }

    private static Mesh GenerateSimpleMesh(MeshGenerationSettings settings)
    {
        // Calculates the total size of the mesh and allocates space for the verticies and triangles //
        int totalVertexCount = (int)Mathf.Pow(settings._VertexCountPerSide + 1, 2);
        int totalTriangleCount = (int)Mathf.Pow(settings._VertexCountPerSide, 2);

        Vector3[] verticies = new Vector3[totalVertexCount];
        int[] triangles = new int[totalTriangleCount * 6];

        // Generates the position of the first vertex in the mesh //
        Vector3 startPosition = new
        (
            (settings._VertexCountPerSide / 2f) * (-settings._DistBetweenVertecies),
            0,
            (settings._VertexCountPerSide / 2f) * (-settings._DistBetweenVertecies)
        );

        Vector3 xOffset = new(settings._DistBetweenVertecies, 0, 0);
        Vector3 zOffset = new(0, 0, settings._DistBetweenVertecies);

        // Indecies of the different information generation //
        int vIndex = 0;
        int tIndex = 0;

        // Calculates the information of the mesh //
        for (int z = 0; z <= settings._VertexCountPerSide; z++)
        {
            for (int x = 0; x <= settings._VertexCountPerSide; x++)
            {
                // Sets the position of the vertex //
                verticies[vIndex] = startPosition + (xOffset * x) + (zOffset * z);
                verticies[vIndex].y = Mathf.PerlinNoise(x / 37f, z / 37f) * 20;
                vIndex++;

                // Calculates the verticies of the triangles //
                if (x < settings._VertexCountPerSide && z < settings._VertexCountPerSide)
                {
                    // Offset of the first vertex //
                    int offset = (int)(z * (settings._VertexCountPerSide + 1) + x);

                    // First triangle of the quad //
                    triangles[tIndex + 0] = offset + 0;
                    triangles[tIndex + 1] = offset + 1 + settings._VertexCountPerSide;
                    triangles[tIndex + 2] = offset + 1;

                    // Second triangle of the quad //
                    triangles[tIndex + 3] = offset + 1;
                    triangles[tIndex + 4] = offset + 1 + settings._VertexCountPerSide;
                    triangles[tIndex + 5] = offset + 2 + settings._VertexCountPerSide;

                    tIndex = tIndex + 6;
                }
            }
        }

        // Creates the mesh and assigns all the generated information before returning //
        Mesh mesh = new()
        {
            vertices = verticies,
            triangles = triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh Create(MeshGenerationSettings settings)
    {
        // Verifies the settings //
        if (settings._VertexCountPerSide > 250)
        {
            Debug.LogError($"MeshGenerationSettings.VertexCountPerSide cannot be higher than 250. Value given {settings._VertexCountPerSide}");
            return null;
        }

        // Creates the mesh //
        Mesh output = GenerateSimpleMesh(settings);

        return output;
    }
}
