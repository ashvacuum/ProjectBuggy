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
        public int yScalar;
        public void OnCreate(ref SystemState state)
        { 
            yScalar = 500;
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (artillery,transform, entity) in SystemAPI.Query<RefRW<ArtilleryMotion>, RefRW<LocalTransform>>()
                         .WithEntityAccess()
                         .WithNone<ArtilleryExplosionTag,DeadComponentTag>())
            {
                if (artillery.ValueRO.TimeLeft >= artillery.ValueRO.TotalTimeToReachTarget)
                {
                    ecb.AddComponent<ArtilleryExplosionTag>(entity);
                    continue;
                }
                var originalPos = artillery.ValueRO.OriginalPosition;
                var targetPos = artillery.ValueRO.TargetPosition;
                var controlPoint = math.lerp(originalPos, targetPos, .3f);
                controlPoint = new float3(controlPoint.x, controlPoint.y + yScalar, controlPoint.z);
                var normalizedDuration = artillery.ValueRO.TimeLeft / artillery.ValueRO.TotalTimeToReachTarget;
                var nextPoint = GetPointOnCurve(normalizedDuration, originalPos, targetPos, controlPoint);
                if (normalizedDuration > 1)
                    transform.ValueRW.Rotation =
                        quaternion.LookRotation(nextPoint - transform.ValueRO.Position, math.up());
                
                transform.ValueRW.Position = nextPoint;
                
                artillery.ValueRW.TimeLeft += SystemAPI.Time.DeltaTime;
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        public float3 GetPointOnCurve(float t, float3 start, float3 end, float3 control)
        {
            // Clamp t between 0 and 1 to ensure valid interpolation
            t = math.clamp(t, 0, 1);
        
            // Quadratic Bezier formula:
            // B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            float oneMinusT = 1f - t;
        
            return oneMinusT * oneMinusT * start +
                   2f * oneMinusT * t * control +
                   t * t * end;
        }
    }
    
    /// <summary>
    /// if existing will cause VFX and an explosion radius that deals damage
    /// </summary>
    public struct ArtilleryExplosionTag : IComponentData {}
}
