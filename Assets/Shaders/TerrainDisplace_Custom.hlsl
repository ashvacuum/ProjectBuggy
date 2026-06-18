#ifndef TERRAINDISPLACE_CUSTOM_INCLUDED
#define TERRAINDISPLACE_CUSTOM_INCLUDED

// Invariant 1: Include our unified shared height calculations
#include "TerrainField.hlsl"

void TerrainDisplace_float(
    float3    worldPos,
    float     frequency,
    float     amplitude,
    float     cliffs,
    Texture2D deformTex,
    SamplerState deformSampler,
    float2    deformUV,
    float     deformDepth,
    out float3 positionWS,
    out float3 normalWS)
{
    float2 xz = worldPos.xz;
    
    // Sample the base mathematical height
    float baseH = TF_SampleHeight(xz, frequency, amplitude, cliffs);
    
    // Sample the Scrolling Brittle Crust texture (R channel: 1.0 = solid, 0.0 = collapsed)
    float crustIntegrity = deformTex.SampleLevel(deformSampler, deformUV, 0).r;
    float finalH = baseH - ((1.0 - crustIntegrity) * deformDepth);
    
    positionWS = float3(worldPos.x, finalH, worldPos.z);

    // Finite difference gradient check for dynamic normals
    const float eps = 0.1; // Reduced epsilon slightly for tighter normal tracking on cliffs
    
    float2 uvPX = deformUV + float2(eps * 0.0005, 0); // Scale texture sample step with epsilon
    float2 uvMX = deformUV - float2(eps * 0.0005, 0);
    float2 uvPZ = deformUV + float2(0, eps * 0.0005);
    float2 uvMZ = deformUV - float2(0, eps * 0.0005);

    float hpx = TF_SampleHeight(xz + float2(eps, 0), frequency, amplitude, cliffs) - ((1.0 - deformTex.SampleLevel(deformSampler, uvPX, 0).r) * deformDepth);
    float hmx = TF_SampleHeight(xz - float2(eps, 0), frequency, amplitude, cliffs) - ((1.0 - deformTex.SampleLevel(deformSampler, uvMX, 0).r) * deformDepth);
    float hpz = TF_SampleHeight(xz + float2(0, eps), frequency, amplitude, cliffs) - ((1.0 - deformTex.SampleLevel(deformSampler, uvPZ, 0).r) * deformDepth);
    float hmz = TF_SampleHeight(xz - float2(0, eps), frequency, amplitude, cliffs) - ((1.0 - deformTex.SampleLevel(deformSampler, uvMZ, 0).r) * deformDepth);
    
    float dhdx = (hpx - hmx) / (2.0 * eps);
    float dhdz = (hpz - hmz) / (2.0 * eps);
    
    normalWS = normalize(float3(-dhdx, 1.0, -dhdz));
}

#endif