using Unity.Mathematics;
using UnityEngine;

namespace ProjectBuggy
{
    [System.Serializable]
    public struct NoiseParams
    {
        public uint Seed;

        [Tooltip("Base spatial frequency. Lower = wider dunes.")]
        public float Frequency;

        [Tooltip("Max terrain height in world units.")]
        public float Amplitude;

        [Range(1, 8)] public int Octaves;

        [Tooltip("Frequency multiplier per octave (~2).")]
        public float Lacunarity;

        [Tooltip("Amplitude falloff per octave (~0.5).")]
        public float Gain;

        public static NoiseParams Default => new NoiseParams
        {
            Seed = 1,
            Frequency = 0.01f,
            Amplitude = 20f,
            Octaves = 4,
            Lacunarity = 2f,
            Gain = 0.5f,
        };
    }

    /// <summary>
    /// Single source of truth for terrain height. Burst-compatible (Unity.Mathematics only),
    /// so the buggy height sampler and the swarm cost field can call the same function later.
    /// The rendered Terrain mesh is just a cache of this function.
    /// </summary>
    public static class TerrainNoise
    {
        /// World XZ position -> height in [0, Amplitude]. Fractal simplex (fbm) for rolling dunes.
        public static float SampleHeight(float2 worldPos, in NoiseParams p)
        {
            float2 origin = SeedOffset(p.Seed);
            float freq = p.Frequency;
            float amp = 1f;
            float sum = 0f;
            float norm = 0f;
            int octaves = math.max(1, p.Octaves);
            for (int i = 0; i < octaves; i++)
            {
                float n = noise.snoise((worldPos + origin) * freq) * 0.5f + 0.5f; // [-1,1] -> [0,1]
                sum += n * amp;
                norm += amp;
                amp *= p.Gain;
                freq *= p.Lacunarity;
            }
            return (sum / norm) * p.Amplitude;
        }

        /// Slope (dHeight/dX, dHeight/dZ) via central difference. Drives buggy momentum + launch later.
        public static float2 SampleGradient(float2 worldPos, in NoiseParams p, float epsilon = 0.5f)
        {
            float hx = SampleHeight(worldPos + new float2(epsilon, 0f), p)
                     - SampleHeight(worldPos - new float2(epsilon, 0f), p);
            float hz = SampleHeight(worldPos + new float2(0f, epsilon), p)
                     - SampleHeight(worldPos - new float2(0f, epsilon), p);
            return new float2(hx, hz) / (2f * epsilon);
        }

        // CreateFromIndex hashes the seed, so 0 is valid (unlike new Random(0)).
        static float2 SeedOffset(uint seed)
        {
            var rng = Unity.Mathematics.Random.CreateFromIndex(seed);
            return rng.NextFloat2(-10000f, 10000f);
        }
    }
}
