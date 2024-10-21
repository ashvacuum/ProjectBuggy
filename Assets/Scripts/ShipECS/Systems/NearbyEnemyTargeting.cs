using System.Linq;
using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    public partial struct NearbyEnemyTargeting : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShipComponent>();
            state.RequireForUpdate<EnemyTrackingComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var nearestEnemyPosition = float.PositiveInfinity;
            foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<ShipComponent>())
            {
                nearestEnemyPosition = SystemAPI.Query<RefRO<LocalTransform>>()
                    .WithAll<EnemyFollowTarget>()
                    .WithNone<NewEnemySpawn>()
                    .Select(enemyTransform => math.distance(playerTransform.ValueRO.Position, (float3)enemyTransform.ValueRO.Position))
                    .Aggregate(nearestEnemyPosition, (current, distance) => math.min(distance, current));

                break;
            }
        }
    }

    public struct EnemyTrackingComponent : IComponentData
    {
        public float3 TrackingTargetPosition;

        public bool IsTargetInRange(float3 player, float radius) =>
            radius >= math.distance(player, TrackingTargetPosition);
    }
}

