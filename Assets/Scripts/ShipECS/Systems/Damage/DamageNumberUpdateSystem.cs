using NonECS.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShipECS.Systems
{
    [BurstCompile]
    public partial struct DamageNumberUpdateSystem : ISystem
    {
        private const float LIFETIME = .5f;
        private const float RISE_HEIGHT = 0f;
        private const float FADE_START = 0.4f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ActiveDamageNumber>();
        }
        
        
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var uiManager = Object.FindFirstObjectByType<DamageNumberUIManager>();
            if (uiManager == null) return;

            foreach (var (damageNumber, entity) in
                     SystemAPI.Query<RefRW<ActiveDamageNumber>>().WithEntityAccess())
            {
                damageNumber.ValueRW.TimeAlive += deltaTime;

                var normalizedTime = damageNumber.ValueRO.TimeAlive / LIFETIME;
                var basePosition = damageNumber.ValueRO.WorldPosition;
                var yOffset = math.sin(normalizedTime * math.PI) * RISE_HEIGHT;

                var position = basePosition + new float3(
                    damageNumber.ValueRO.RandomOffset.x,
                    0,
                    damageNumber.ValueRO.RandomOffset.y);

            

                // Calculate fade
                var alpha = 1f;
                if (normalizedTime > FADE_START)
                {
                    var fadeProgress = (normalizedTime - FADE_START) / (1 - FADE_START);
                    alpha = 1 - fadeProgress;
                }

                // Update UI position and alpha
                uiManager.UpdateDamageNumber(damageNumber.ValueRO.UIElementId, position, alpha);

                // Destroy when lifetime is over
                if (!(damageNumber.ValueRO.TimeAlive >= LIFETIME)) continue;
                uiManager.RemoveDamageNumber(damageNumber.ValueRO.UIElementId);
                ecb.DestroyEntity(entity);
            }
        }
    }
}