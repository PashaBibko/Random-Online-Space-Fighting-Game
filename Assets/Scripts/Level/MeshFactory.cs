using UnityEngine;
using UnityEngine.UIElements;

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

    public struct MeshGenData
    {
        public Vector3[] verticies;
        public Color[] colors;
        public int[] triangles;

        public MeshGenData(Vector3[] verticies, Color[] colors, int[] triangles) : this()
        {
            this.verticies = verticies;
            this.colors = colors;
            this.triangles = triangles;
        }
    }

    private static Color GenColor(float a, float b, float c)
    {
        float avg = (a + b + c) / 3f;
        float e = 0.35f;

        if (avg < e)
        {
            avg = avg * (1 / e);

            Color d = new(6 / 255f, 75 / 255f, 0 / 255f);
            Color l = new(96 / 255f, 255 / 255f, 82 / 255f);

            return Color.Lerp(l, d, avg);
        }

        if (avg > 0.7f)
        {
            return Color.white;
        }

        return Color.gray;
    }

    private static float SampleNoise(Vector3 pos, Vector2 offset, float scale, ValleyNode valley)
    {
        float minDistance = Mathf.Infinity;

        Vector3 offset3D = new(offset.x, 0, offset.y);
        Vector3 pos3D = pos + offset3D;

        // Finds the shortest distance to any of the nodes or the lines between them //
        valley.CallFuncOnNodes((ValleyNode node) =>
        {
            // Returns early if it is the starting node //
            if (node == valley) { return; }

            // Gets the start and end positions of the line //
            Vector3 start = node.Position() * 200;
            Vector3 end = node.Creator().Position() * 200;

            float distance = Geo.DistanceFromLine(pos3D, start, end);
            minDistance = Mathf.Min(minDistance, distance);
        });

        float multi = 0;

        if (minDistance < 100)
        {
            multi = 0.4f;
        }

        else if (minDistance < 200)
        {
            float d = minDistance - 100;
            multi = 0.4f + d / 100;
        }

        else
        {
            multi = 1.4f;
        }

        return SamplePerlin(pos, offset, scale) * multi;
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
            Vector2 p = new(pos.x, pos.z);
            float perlin = Perlin.Sample(p / scale * freq, 23117);
            total += perlin * amp;

            max += amp;

            freq *= 2f;
            amp *= 0.5f;
        }

        return total / max;
    }

    public static MeshGenData Create(MeshGenerationSettings settings, Vector2 offset, ValleyNode valley)
    {
        // Allocates all of the memory for the mesh items //
        int length = (int)Mathf.Pow(settings._VertexCountPerSide, 2) * 6;

        MeshGenData data = new
        (
            verticies:  new Vector3[length],
            colors:     new Color[length],
            triangles:  new int[length]
        );

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
                bl.y = SampleNoise(bl, offset, 587, valley);
                br.y = SampleNoise(br, offset, 587, valley);
                tl.y = SampleNoise(tl, offset, 587, valley);
                tr.y = SampleNoise(tr, offset, 587, valley);

                // Creates the colors before y-modification //
                Color c1 = GenColor(bl.y, tl.y, tr.y);
                Color c2 = GenColor(bl.y, tr.y, br.y);

                bl.y *= 350;
                br.y *= 350;
                tl.y *= 350;
                tr.y *= 350;

                // Triangle 1 vertex positions //
                data.verticies[index + 0] = bl;
                data.verticies[index + 1] = tl;
                data.verticies[index + 2] = tr;

                // Triangle 1 color //
                data.colors[index + 0] = c1;
                data.colors[index + 1] = c1;
                data.colors[index + 2] = c1;

                // Triangle 1 indecies //
                data.triangles[index + 0] = index + 0;
                data.triangles[index + 1] = index + 1;
                data.triangles[index + 2] = index + 2;

                // Triangle 2 vertex positions //
                data.verticies[index + 3] = bl;
                data.verticies[index + 4] = tr;
                data.verticies[index + 5] = br;

                // Triangle 2 color //
                data.colors[index + 3] = c2;
                data.colors[index + 4] = c2;
                data.colors[index + 5] = c2;

                // Triangle 2 indecies //
                data.triangles[index + 3] = index + 3;
                data.triangles[index + 4] = index + 4;
                data.triangles[index + 5] = index + 5;

                index += 6;
            }
        }

        return data;
    }
}
