using UnityEngine;

public static class Perlin
{
    // Fancy lerping function //
    static float Lerp(float a, float b, float w)
        => (b - a) * (3f - w * 2f) * w * w + a;

    private static float GradientVector(int cx, int cy, int seed)
    {
        // Honestly this function still confuses me when I know what it does so just imagine it as a black box //
        // Please don't try to understand it //

        const int w = 8 * sizeof(int);
        const int s = w / 2;

        int a = cx + seed;
        int b = cy + seed;

        // RANDOM CONSTANTS TIME BABYYYYYYYYY //
        unchecked
        {
            a *= (int)328415744;

            b ^= a << s | a >> (w - s);
            b *= (int)191152071;

            a ^= b << s | b >> (w - s);
            a *= (int)2048419325;

            // Returns a times this abomination //
            return a * 1.4629180792671596e-9f;
        }
    }

    private static float DotGridGradient(int cx, int cy, Vector2 pos, int seed)
    {
        float angle = GradientVector(cx, cy, seed);

        float gx = Mathf.Sin(angle);
        float gy = Mathf.Cos(angle);

        float dx = pos.x - (float)cx;
        float dy = pos.y - (float)cy;

        return (dx * gx + dy * gy);
    }


    public static float Sample(Vector2 pos, int seed)
    {
        // Adds a large constant as the algorithim breaks with negative values //
        pos = pos + new Vector2(1000, 1000);

        // Determines the grid cell coordinates //
        int x0 = (int)pos.x;
        int x1 = x0 + 1;

        int y0 = (int)pos.y;
        int y1 = y0 + 1;

        // Interpolation weights //
        float sx = pos.x - x0;
        float sy = pos.y - y0;

        // Calculates the dot grid gradients //
        float g0 = DotGridGradient(x0, y0, pos, seed);
        float g1 = DotGridGradient(x1, y0, pos, seed);
        float g2 = DotGridGradient(x0, y1, pos, seed);
        float g3 = DotGridGradient(x1, y1, pos, seed);

        // Lerps between the gradients to find the results //
        float l0 = Lerp(g0, g1, sx);
        float l1 = Lerp(g2, g3, sx);
        return Mathf.Clamp01(Lerp(l0, l1, sy) + 0.5f);
    }
}
