using System.Linq;
using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateBefore(typeof(ProjectileFiringSystem))]
    public partial struct NearbyEnemyTargetingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<EnemyTrackingComponent>();
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var nearestEnemyPosition = float.PositiveInfinity;
            

            var tracking = SystemAPI.GetSingleton<EnemyTrackingComponent>();
            tracking.TrackingTargetPosition =
                new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>())
            {
                foreach (var enemyTransform in SystemAPI.Query<RefRO<LocalTransform>>()
                             .WithAll<EnemyFollowTarget>()
                             .WithNone<NewEnemySpawn>())
                {
                    var distance = math.distance(enemyTransform.ValueRO.Position, playerTransform.ValueRO.Position);
                    var pastDistance = math.distance(tracking.TrackingTargetPosition, playerTransform.ValueRO.Position);
                    if (pastDistance < distance)
                    {
                        tracking.TrackingTargetPosition = enemyTransform.ValueRO.Position;
                    }
                    
                }

                Debug.Log($"Tracking {tracking.TrackingTargetPosition.x},{tracking.TrackingTargetPosition.y},{tracking.TrackingTargetPosition.z}");
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

