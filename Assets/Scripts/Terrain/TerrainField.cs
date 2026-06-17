using Unity.Mathematics;
using UnityEngine;

namespace ProjectBuggy
{
    [System.Serializable]
    public struct TerrainParams
    {
        [Tooltip("Global feature-size scale. Lower = wider features.")]
        public float Frequency;

        [Tooltip("Height scale (world units).")]
        public float Amplitude;

        [Range(0f, 1f)]
        [Tooltip("0 = smooth dunes, 1 = terraced canyons. Driven by world-mutation upgrades.")]
        public float CliffsModifier;

        public static TerrainParams Default => new TerrainParams
        {
            Frequency = 1f,
            Amplitude = 1f,
            CliffsModifier = 0f,
        };
    }

    /// <summary>
    /// Shared terrain height field — CPU half. MUST match Assets/Shaders/TerrainField.hlsl line-for-line
    /// (invariant 1). Sum-of-sines: CPU physics samples this; the GPU floor displaces from the .hlsl.
    /// The two match to within sin() precision — negligible for HEIGHT (sub-mm). That's exactly why
    /// this is safe where a sin-based HASH would not be: a hash amplifies the tiny diff, a height does not.
    /// Pure Unity.Mathematics, Burst-clean. The "+ deformation" seam (Phase 6) goes inside SampleHeight.
    /// </summary>
    public static class TerrainField
    {
        public static float SampleHeight(float2 p, in TerrainParams t)
        {
            p *= t.Frequency;
            float warp   = math.sin(p.x * 0.05f) * 3.0f;                        // domain warp -> irregular dunes
            float macro  = math.sin(p.x * 0.01f) * math.cos(p.y * 0.01f) * 12.0f
                         + math.sin((p.y + warp) * 0.013f) * 6.0f;              // macro dunes
            float ripple = math.sin(p.x * 0.40f) * 0.20f
                         + math.sin(p.y * 0.37f) * 0.15f;                       // micro ripples
            float baseH  = (macro + ripple) * t.Amplitude;
            float terraced = SoftTerrace(baseH, 5.0f);
            return math.lerp(baseH, terraced, t.CliffsModifier);
            // Phase 6 seam: + DeformationField.Sample(p)
        }

        // Softened terrace: flat plateaus joined by steep-but-finite ramps (NOT hard round() walls).
        static float SoftTerrace(float h, float step)
        {
            float level = math.floor(h / step) * step;
            float f = (h - level) / step;                  // 0..1 within the step
            return level + math.smoothstep(0.35f, 0.65f, f) * step;
        }

        public static float2 SampleGradient(float2 p, in TerrainParams t, float eps = 0.5f)
        {
            float hx = SampleHeight(p + new float2(eps, 0f), t) - SampleHeight(p - new float2(eps, 0f), t);
            float hz = SampleHeight(p + new float2(0f, eps), t) - SampleHeight(p - new float2(0f, eps), t);
            return new float2(hx, hz) / (2f * eps);
        }

        public static float3 SampleNormal(float2 p, in TerrainParams t)
        {
            float2 g = SampleGradient(p, t);
            return math.normalize(new float3(-g.x, 1f, -g.y));
        }
    }
}
