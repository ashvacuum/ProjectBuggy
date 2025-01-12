using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ShipECS.Systems.Artillery
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    partial struct ArtilleryTargetingAndFiringSystem : ISystem
    {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CalculatePositionsJob().ScheduleParallel();
            
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<ArtilleryQueue>(out var artilleryQueue);
            if (hasBuffer)
            {
                foreach (var artillery in artilleryQueue)
                {
                    //state.EntityManager.Instantiate() TODO: 
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

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
    
    public static class PositionCalculator
    {
        
    }
}
