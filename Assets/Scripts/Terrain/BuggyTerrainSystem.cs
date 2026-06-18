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
    /// Pure ECS, reuses the existing player entity + VehicleMovementSystem. The "suspension
    /// bed" (step 1) — real spring/look-ahead suspension layers on top of this later.
    /// </summary>
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateAfter(typeof(VehicleMovementSystem))]
    public partial struct BuggyTerrainSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cfg = SystemAPI.GetSingleton<TerrainConfig>();
            float dt = SystemAPI.Time.DeltaTime;
            foreach (var (transform, vehicle) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<VehiclePhysicsData>>().WithAll<PlayerTag>())
            {
                var p = transform.ValueRO.Position;
                // World-space absolute (invariant 4): sample the shared formula at the buggy's XZ.
                float targetY = TerrainField.SampleHeight(p.xz, cfg.Params);

                // ponytail: exponential ride-smoothing stands in for real suspension (slice 1.5).
                // Frame-rate independent; tracks low-freq dunes, damps micro-ripple chatter, and
                // limits the per-frame vertical correction (invariant 6). 0 = hard snap.
                float k = vehicle.ValueRO.RideSmoothing;
                p.y = k > 0f ? math.lerp(p.y, targetY, 1f - math.exp(-k * dt)) : targetY;

                transform.ValueRW.Position = p;
            }
        }
    }
}
