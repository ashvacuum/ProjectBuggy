using Authoring;
using ShipECS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectBuggy
{
    /// <summary>
    /// Pins the player/buggy entity to the terrain surface, sampling the same TerrainNoise the
    /// rendered Unity Terrain was built from. Runs after movement sets X/Z; this owns Y.
    /// Pure ECS, reuses the existing player entity + CharacterMovementSystem. The "suspension
    /// bed" (step 1) — real spring/look-ahead suspension layers on top of this later.
    /// </summary>
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateAfter(typeof(CharacterMovementSystem))]
    public partial struct BuggyTerrainSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainNoiseSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var terrain = SystemAPI.GetSingleton<TerrainNoiseSingleton>();
            foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerTag>())
            {
                var p = transform.ValueRO.Position;
                var xz = new float2(p.x - terrain.Origin.x, p.z - terrain.Origin.z);
                p.y = terrain.Origin.y + TerrainNoise.SampleHeight(xz, terrain.Noise);
                transform.ValueRW.Position = p;
            }
        }
    }
}
