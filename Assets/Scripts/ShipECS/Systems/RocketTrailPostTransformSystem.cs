using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
//[UpdateInGroup(typeof())]
[UpdateAfter(typeof(TransformSystemGroup))]
public partial struct RocketTrailPostTransformSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VFXThrustersSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        VFXThrustersSingleton vfxThrustersSingleton = SystemAPI.GetSingletonRW<VFXThrustersSingleton>().ValueRW;

        ShipSetVFXDataJob shipSetVFXDataJob = new ShipSetVFXDataJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ThrustersData = vfxThrustersSingleton.Manager.Datas,
        };
        state.Dependency = shipSetVFXDataJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct ShipSetVFXDataJob : IJobEntity
    {
        public float DeltaTime;
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<VFXRocketData> ThrustersData;
            
        private void Execute(in LocalTransform transform, in RocketTrailData ship)
        {
            if (ship.RocketVFXIndex >= 0)
            {
                ref RocketData shipData = ref ship.RocketData.Value;
                VFXRocketData thrusterData = ThrustersData[ship.RocketVFXIndex];
                thrusterData.Position =
                    transform.Position + math.mul(transform.Rotation, shipData.ThrusterLocalPosition);
                thrusterData.Direction = math.mul(transform.Rotation, -math.forward());
                ThrustersData[ship.RocketVFXIndex] = thrusterData;
            }
        }
    }
}
