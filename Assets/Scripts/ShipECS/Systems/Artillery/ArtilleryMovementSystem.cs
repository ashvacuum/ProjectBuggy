using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems.Artillery
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateAfter(typeof(ArtilleryTargetingAndFiringSystem))]
    public partial struct ArtilleryMovementSystem : ISystem
    {
        private int yScalar;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            yScalar = 500;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
/*
            foreach (var (artillery, transform, entity) in SystemAPI
                         .Query<RefRW<ArtilleryMotion>, RefRW<LocalTransform>>()
                         .WithEntityAccess()
                         .WithNone<ArtilleryExplosionTag, DeadComponentTag>())
            {
                if (artillery.TimeLeft >= artillery.TotalTimeToReachTarget)
                {
                    ecb.AddComponent<ArtilleryExplosionTag>(entity);
                    continue;
                }

                var originalPos = artillery.OriginalPosition;
                var targetPos = artillery.TargetPosition;
                var controlPoint = math.lerp(originalPos, targetPos, .3f);
                controlPoint = new float3(controlPoint.x, controlPoint.y + yScalar, controlPoint.z);
                var normalizedDuration = artillery.TimeLeft / artillery.TotalTimeToReachTarget;
                var nextPoint = GetPointOnCurve(normalizedDuration, originalPos, targetPos, controlPoint);
                if (normalizedDuration < 1)
                    transform.Rotation =
                        quaternion.LookRotation(nextPoint - transform.Position, math.up());
                else
                {
                    quaternion.LookRotation(-transform.Up(), math.up());
                }

                transform.Position = nextPoint;

                artillery.TimeLeft += SystemAPI.Time.DeltaTime;
            }
*/
            var artilleryMotionJob = new ArtilleryMovementJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged),
                VerticalScalar = yScalar
            };

            state.Dependency = artilleryMotionJob.Schedule(state.Dependency);
            state.Dependency.Complete();

        }

        private static float3 GetPointOnCurve(float t, float3 start, float3 end, float3 control)
        {
            // Clamp t between 0 and 1 to ensure valid interpolation
            t = math.clamp(t, 0, 1);
            t  *= t;

            // Quadratic Bezier formula:
            // B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            var oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * start +
                   2f * oneMinusT * t * control +
                   t * t * end;
        }
        
        [BurstCompile]
        [WithNone(typeof(ArtilleryExplosionTag),typeof(DeadComponentTag))]
        public partial struct ArtilleryMovementJob : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public float DeltaTime;
            public float VerticalScalar;
            
            public void Execute(Entity entity, ref ArtilleryMotion artillery, ref LocalTransform transform)
            {
                if (artillery.TimeLeft >= artillery.TotalTimeToReachTarget)
                {
                    ECB.AddComponent<ArtilleryExplosionTag>(entity);
                    return;
                }

                var originalPos = artillery.OriginalPosition;
                var targetPos = artillery.TargetPosition;
                var controlPoint = math.lerp(originalPos, targetPos, .3f);
                controlPoint = new float3(controlPoint.x, controlPoint.y + VerticalScalar, controlPoint.z);
                
                var normalizedDuration = artillery.TimeLeft / artillery.TotalTimeToReachTarget;
                var scaleDuration = math.clamp(normalizedDuration / 0.3f, 0, 1);
                transform.Scale = math.lerp(0, 1, scaleDuration * scaleDuration);
                var nextPoint = GetPointOnCurve(normalizedDuration, originalPos, targetPos, controlPoint);
                if (normalizedDuration < 1)
                    transform.Rotation =
                        quaternion.LookRotation(nextPoint - transform.Position, math.up());
                else
                {
                    quaternion.LookRotation(-transform.Up(), math.up());
                }

                transform.Position = nextPoint;

                artillery.TimeLeft += DeltaTime;
            }

        }
    }

    /// <summary>
    /// if existing will cause VFX and an explosion radius that deals damage
    /// </summary>
    public struct ArtilleryExplosionTag : IComponentData
    {
    }

    
}
