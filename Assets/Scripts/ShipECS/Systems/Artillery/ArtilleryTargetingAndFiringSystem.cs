using Authoring;
using NonECS.BaseWeapons;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ShipECS.Systems.Artillery
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    partial struct ArtilleryTargetingAndFiringSystem : ISystem
    {
        private int currentTargetedLocationIndex;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            currentTargetedLocationIndex = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CalculatePositionsJob().ScheduleParallel();
            
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<ArtilleryQueue>(out var artilleryQueue);
            var hasSpawnBuffer = SystemAPI.TryGetSingletonBuffer<ProjectileSpawnerComponent>(out var projectileSpawner);
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            if (hasBuffer)
            {
                foreach (var artilleryFiringAspect in SystemAPI.Query<ArtilleryFiringAspect>())
                {

                    foreach (var artillery in artilleryQueue)
                    {
                        if (!ProjectileHelper.TryGetEntityFromWeaponClass(projectileSpawner, WeaponClass.Artillery,
                                out var artilleryPrefab)) break;
                        var newEntity = ecb.Instantiate(artilleryPrefab);

                        var computedSpeed = CalculateLerpT(artilleryFiringAspect.TotalSpeed, 100, 1000);
                        var computedDuration = GetScaledDuration(computedSpeed, 2.0f);
                        ecb.AddComponent(newEntity, new ArtilleryMotion()
                        {
                            OriginalPosition = artilleryFiringAspect.Position,
                            TargetPosition = artilleryFiringAspect.GetPosition(ref currentTargetedLocationIndex),
                            TimeLeft = 0,
                            TotalTimeToReachTarget =  computedDuration
                        });
                        currentTargetedLocationIndex++;

                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        private float CalculateLerpT(float value, float minValue, float maxValue)
        {
            // Simple formula: (value - min) / (max - min)
            return (value - minValue) / (maxValue - minValue);
        }

        private float GetScaledDuration(float t, float baseDuration)
        {
            if (t == 0) return float.MaxValue; // Prevent division by zero
            return baseDuration / t;
        }
    }
    
    [BurstCompile]
    public partial struct CalculatePositionsJob : IJobEntity
    {
        void Execute(ArtilleryFiringAspect artillery)
        {
            artillery.CalculatePositions();
        }
    }

    public struct ArtilleryTarget : IBufferElementData
    {
        public float3 TargetLocation;
    }
    
    public struct ArtilleryMotion : IComponentData
    {
        public float3 TargetPosition;
        public float3 OriginalPosition;
        public float TimeLeft;
        public float TotalTimeToReachTarget;
    }
}
