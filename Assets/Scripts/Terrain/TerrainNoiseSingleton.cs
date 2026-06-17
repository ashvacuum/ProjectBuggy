using Unity.Entities;
using Unity.Mathematics;

namespace ProjectBuggy
{
    /// <summary>
    /// Bridges the render-side NoiseParams into ECS so gameplay systems sample the exact same
    /// terrain the Unity Terrain mesh was built from. Pushed by TerrainGenerator at startup.
    /// </summary>
    public struct TerrainNoiseSingleton : IComponentData
    {
        public NoiseParams Noise;
        public float3 Origin; // terrain GameObject world position; heights are relative to it
    }
}
