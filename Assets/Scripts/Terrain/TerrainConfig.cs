using Unity.Entities;

namespace ProjectBuggy
{
    /// <summary>
    /// Single source of truth for live terrain params on the ECS side (invariant 1).
    /// Burst physics samplers — BuggyTerrainSystem now, suspension + swarm slope-repulsion later —
    /// read this to call TerrainField.SampleHeight. The same values feed the GPU material so the
    /// surface the buggy rides matches the surface the shader displaces.
    /// World-mutation upgrade cards (Phase 4.2) write Params here.
    /// </summary>
    public struct TerrainConfig : IComponentData
    {
        public TerrainParams Params;
    }
}
