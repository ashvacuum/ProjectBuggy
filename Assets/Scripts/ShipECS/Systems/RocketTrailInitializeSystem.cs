using Authoring;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShipECS.Systems
{
    partial struct RocketTrailInitializeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<VFXThrustersSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            VFXThrustersSingleton vfxThrustersSingleton = SystemAPI.GetSingletonRW<VFXThrustersSingleton>().ValueRW;
        
            ShipInitializeJob shipInitializeJob = new ShipInitializeJob
            {
                ThrustersManager = vfxThrustersSingleton.Manager,
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged)
            };

            state.Dependency = shipInitializeJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }

    [BurstCompile]
    [WithAll(typeof(RocketInitTag))]
    [WithNone(typeof(DeadComponentTag))]
    public partial struct ShipInitializeJob : IJobEntity
    {
        public VFXManagerParented<VFXRocketData> ThrustersManager;
        
        public EntityCommandBuffer ECB;
 
        private void Execute(in Entity entity, ref RocketTrailData rocket)
        {
            RocketData shipData = rocket.RocketData.Value;

            rocket.RocketVFXIndex = ThrustersManager.Create();
                
            if (rocket.RocketVFXIndex >= 0)
            {
                ThrustersManager.Datas[rocket.RocketVFXIndex] = new VFXRocketData()
                {
                    Color = new float3(0,0,1), //blue
                    Size = shipData.ThrusterSize,
                    Length = shipData.ThrusterLength,
                };
            };

            Debug.Log("Initialized");
            
            ECB.RemoveComponent<RocketInitTag>(entity);
        }
    }
    
}