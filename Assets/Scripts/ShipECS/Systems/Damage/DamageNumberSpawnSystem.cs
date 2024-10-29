using NonECS.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShipECS.Systems
{
    [BurstCompile]
    public partial struct DamageNumberSpawnSystem : ISystem
    {
        private Unity.Mathematics.Random _random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            state.RequireForUpdate<DamageNumberUICounter>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
        
            var uiManager = Object.FindFirstObjectByType<DamageNumberUIManager>();
            if (uiManager == null) return;
            
            _random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
            foreach (var (request, entity) in SystemAPI.Query<RefRO<DamageNumberRequest>>().WithEntityAccess())
            {
                var randomOffset = _random.NextFloat2(new float2(-0.5f, 0.5f), new float2(0.5f, 1f));
            
                // Create UI Element and get its ID
                var uiElementId = uiManager.CreateDamageNumber(
                    request.ValueRO.DamageAmount, 
                    request.ValueRO.WorldPosition
                );
            
                // Create ECS entity to track this damage number
                var damageNumberEntity = ecb.CreateEntity();
                ecb.AddComponent(damageNumberEntity, new ActiveDamageNumber
                {
                    DamageAmount = request.ValueRO.DamageAmount,
                    TimeAlive = 0f,
                    WorldPosition = request.ValueRO.WorldPosition,
                    RandomOffset = randomOffset,
                    UIElementId = uiElementId
                });

                ecb.DestroyEntity(entity);
            }
        }
    }
}