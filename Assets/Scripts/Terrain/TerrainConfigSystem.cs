using Unity.Burst;
using Unity.Entities;

namespace ProjectBuggy
{
    /// <summary>
    /// Owns the TerrainConfig singleton. Seeds it once with TerrainParams.Default so Burst samplers
    /// can read terrain params with no managed dependency. Pure ECS bootstrap — replaces the old
    /// TerrainGenerator.PushNoiseToEcs Mono path. One-shot: disables itself after seeding.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TerrainConfigSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(e, new TerrainConfig { Params = TerrainParams.Default });
            state.Enabled = false; // seeded; nothing to do per frame
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }
    }
}
