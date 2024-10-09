using Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ShipECS.Systems
{
    
    public struct HealthChangedTag : IComponentData {}
    public struct HealthChangeEvent : IComponentData
    {
        public float NewHealth;
    }
    public partial struct HealthNotifySystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            // Check for entities with the HealthChangedTag and update UI
            foreach (var (health, entity) in SystemAPI.Query<RefRO<HealthComponent>>().WithAll<HealthChangedTag>().WithAll<ShipComponent>().WithEntityAccess())
            {
                // Update the UI with the new health value
                // e.g., call a MonoBehaviour to update UI
                GameSceneEvents.Instance.UpdateHealth(health.ValueRO.HealthPercent * 100);

                // Remove the tag after processing
                //state.EntityManager.RemoveComponent<HealthChangedTag>(entity);
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
    public partial struct HealthSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (health, entity) in SystemAPI.Query<RefRW<HealthComponent>>().WithEntityAccess())
            {
                if (health.ValueRO.CurrentNextTimeToTakeDamage > 0)
                {
                    health.ValueRW.CurrentNextTimeToTakeDamage -= SystemAPI.Time.DeltaTime;
                }
                
                if (Mathf.Approximately(health.ValueRO.CurrentHealth, health.ValueRO.PreviousHealth)) continue;
                
                var eventEntity = ecb.CreateEntity();
                ecb.AddComponent(eventEntity, new HealthChangeEvent
                {
                    NewHealth = health.ValueRO.CurrentHealth
                });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
