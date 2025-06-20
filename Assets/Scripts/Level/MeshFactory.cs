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

    private static Color GenColor(float a, float b, float c)
    {
        float avg = (a + b + c) / 3f;
        avg = 1 - (avg + 20) / 40;

        Color d = new(6 / 255f, 75 / 255f, 0 / 255f);
        Color l = new(96 / 255f, 255 / 255f, 82 / 255f);

        return Color.Lerp(l, d, avg);
    }

    private static float SamplePerlin(Vector3 pos, Vector2 offset, float scale)
    {
        pos.x += offset.x;
        pos.z += offset.y;

        float total = 0f;
        float freq = 1f;
        float amp = 1f;
        float max = 0f;

        for (int i = 0; i < 3; i++)
        {
            float perlin = Mathf.PerlinNoise(pos.x / scale * freq, pos.z / scale * freq);
            total += perlin * amp;

            max += amp;

            freq *= 2f;
            amp *= 0.5f;
        }

        return total / max;
    }

    private static Mesh GenerateSimpleMesh(MeshGenerationSettings settings, Vector2 offset)
    {
        // Allocates all of the memory for the mesh items //
        int length = (int)Mathf.Pow(settings._VertexCountPerSide, 2) * 6;

        Vector3[] verticies = new Vector3[length];
        Color[] colors = new Color[length];
        int[] triangles = new int[length];

        // Calculates the start position of the mesh with the offsets //
        Vector3 start = new
        (
            -settings._DistBetweenVertecies * (settings._VertexCountPerSide / 2f),
            0,
            -settings._DistBetweenVertecies * (settings._VertexCountPerSide / 2f)
        );

        Vector3 xOffset = new(settings._DistBetweenVertecies, 0, 0);
        Vector3 zOffset = new(0, 0, settings._DistBetweenVertecies);

        int index = 0;

        float min = Mathf.Infinity;
        float max = Mathf.NegativeInfinity;

        // Calculates the information for each triangle in the mesh //
        for (int z = 0; z < settings._VertexCountPerSide; z++)
        {
            for (int x = 0; x < settings._VertexCountPerSide; x++)
            {
                // The 4 vertex locations (x and z) //
                Vector3 bl = start + (x * xOffset) + (z * zOffset);
                Vector3 br = bl + xOffset;
                Vector3 tl = bl + zOffset;
                Vector3 tr = br + zOffset;

                // Calculates their respective heights //
                bl.y = SamplePerlin(bl, offset, 113) * 60f - 30f;
                br.y = SamplePerlin(br, offset, 113) * 60f - 30f;
                tl.y = SamplePerlin(tl, offset, 113) * 60f - 30f;
                tr.y = SamplePerlin(tr, offset, 113) * 60f - 30f;

                min = Mathf.Min(min, bl.y);
                max = Mathf.Max(max, bl.y);

                // Triangle 1 vertex positions //
                verticies[index + 0] = bl;
                verticies[index + 1] = tl;
                verticies[index + 2] = tr;

                // Triangle 1 color //
                Color c1 = GenColor(bl.y, tl.y, tr.y);
                colors[index + 0] = c1;
                colors[index + 1] = c1;
                colors[index + 2] = c1;

                // Triangle 1 indecies //
                triangles[index + 0] = index + 0;
                triangles[index + 1] = index + 1;
                triangles[index + 2] = index + 2;

                // Triangle 2 vertex positions //
                verticies[index + 3] = bl;
                verticies[index + 4] = tr;
                verticies[index + 5] = br;

                // Triangle 2 color //
                Color c2 = GenColor(bl.y, tr.y, br.y);
                colors[index + 3] = c2;
                colors[index + 4] = c2;
                colors[index + 5] = c2;

                // Triangle 2 indecies //
                triangles[index + 3] = index + 3;
                triangles[index + 4] = index + 4;
                triangles[index + 5] = index + 5;

                index += 6;
            }
        }

        Debug.Log($"Min: [{min}], Max: [{max}]");

        // Creates the mesh and assigns all the generated information before returning //
        Mesh mesh = new()
        {
            vertices = verticies,
            triangles = triangles,
            colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Mesh Create(MeshGenerationSettings settings, Vector2 offset)
    {
        // Verifies the settings //
        if (settings._VertexCountPerSide > 100)
        {
            Debug.LogError($"MeshGenerationSettings.VertexCountPerSide cannot be higher than 250. Value given {settings._VertexCountPerSide}");
            return null;
        }

        // Creates the mesh //
        Mesh output = GenerateSimpleMesh(settings, offset);

        return output;
    }
}
