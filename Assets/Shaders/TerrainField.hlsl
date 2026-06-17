#ifndef TERRAINFIELD_INCLUDED
#define TERRAINFIELD_INCLUDED

// =====================================================================================
// Shared terrain height field — GPU half. MUST match TerrainField.cs line-for-line
// (invariant 1). GPU floor displacement reads this; CPU physics reads the .cs.
// Matches to within sin() precision (negligible for height). Sum-of-sines on purpose.
// =====================================================================================

// Softened terrace: flat plateaus joined by steep-but-finite ramps (NOT hard round() walls).
float TF_SoftTerrace(float h, float step)
{
    float level = floor(h / step) * step;
    float f = (h - level) / step;                  // 0..1 within the step
    return level + smoothstep(0.35, 0.65, f) * step;
}

// p = world XZ. (frequency, amplitude, cliffs) mirror TerrainParams in TerrainField.cs.
float TF_SampleHeight(float2 p, float frequency, float amplitude, float cliffs)
{
    p *= frequency;
    float warp   = sin(p.x * 0.05) * 3.0;                          // domain warp -> irregular dunes
    float macro  = sin(p.x * 0.01) * cos(p.y * 0.01) * 12.0
                 + sin((p.y + warp) * 0.013) * 6.0;                // macro dunes
    float ripple = sin(p.x * 0.40) * 0.20
                 + sin(p.y * 0.37) * 0.15;                         // micro ripples
    float baseH  = (macro + ripple) * amplitude;
    float terraced = TF_SoftTerrace(baseH, 5.0);
    return lerp(baseH, terraced, cliffs);
    // Phase 6 seam: + deformation.Sample(p)
}

float3 TF_SampleNormal(float2 p, float frequency, float amplitude, float cliffs)
{
    const float eps = 0.5;
    float hx = TF_SampleHeight(p + float2(eps, 0), frequency, amplitude, cliffs)
             - TF_SampleHeight(p - float2(eps, 0), frequency, amplitude, cliffs);
    float hz = TF_SampleHeight(p + float2(0, eps), frequency, amplitude, cliffs)
             - TF_SampleHeight(p - float2(0, eps), frequency, amplitude, cliffs);
    float2 g = float2(hx, hz) / (2.0 * eps);
    return normalize(float3(-g.x, 1.0, -g.y));
}

#endif
