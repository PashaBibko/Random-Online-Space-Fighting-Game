using UnityEngine;

[CreateAssetMenu(menuName = "Custom/FloatFunction/Perlin")]
public class PerlinHeightmap : HeightMapFunction
{
    [Header("Settings")]
    [SerializeField, Range(1f, 8f)] int m_Octaves;
    [SerializeField] float m_Multiplier;
    [SerializeField, Range(0f, 1f)] float m_Persistence;
    [SerializeField, Range(1f, 4f)] float m_Lacunarity;

    public override float SampleHeight(Vector2 location, int seed)
    {
        float total = 0f;
        float amp = 1f;
        float frq = 1f;

        // Loops once for every octave //
        for (int i = 1; i <= m_Octaves; i++)
        {
            // Samples the perlin noise and adds it to the total //
            float perlin = Perlin.Sample(location * frq, seed);
            total += perlin * amp;

            // Modifies the amplitude and frequency for the next octave //
            amp *= m_Persistence;
            frq *= m_Lacunarity;
        }

        // Divides by the octave count to keep normalised when the octave count changes //
        total /= m_Octaves;
        return total * m_Multiplier;
    }
}
