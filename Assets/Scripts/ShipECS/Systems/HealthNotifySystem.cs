using Authoring;
using NonECS.UI;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HealthChangeEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);


            foreach (var (healthTag, tagEntity) in SystemAPI.Query<RefRO<HealthChangeEvent>>().WithEntityAccess())
            {
                //Debug.Log("Updating Health");
                GameSceneEvents.Instance.UpdateHealth(healthTag.ValueRO.NewHealth);

                // Destroy Entity
                ecb.DestroyEntity(tagEntity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (health, transform,entity) in SystemAPI.Query<RefRW<HealthComponent>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (health.ValueRO.CurrentNextTimeToTakeDamage > 0)
                {
                    health.ValueRW.CurrentNextTimeToTakeDamage -= SystemAPI.Time.DeltaTime;
                }

                var difference = math.abs(health.ValueRO.CurrentHealth - health.ValueRO.PreviousHealth);
                if (difference < 0.1) continue;
                
                var eventEntity = ecb.CreateEntity();
                
                if (state.EntityManager.HasComponent<PlayerTag>(entity))
                {
                    ecb.AddComponent(eventEntity, new HealthChangeEvent
                    {
                        NewHealth = health.ValueRO.HealthPercent
                    });
                } else
                {
                    ecb.AddComponent(eventEntity, new DamageNumberRequest()
                    {
                        DamageAmount = difference,
                        WorldPosition = transform.ValueRO.Position
                    });
                }

                health.ValueRW.PreviousHealth = health.ValueRO.CurrentHealth;

                
                
                if (health.ValueRO.CurrentHealth <= 0)
                {
                    ecb.AddComponent<DeadComponentTag>(entity);
                }
                
                
                //Debug.Log($"Created Health Change Tag: {health.ValueRO.CurrentHealth} {health.ValueRO.PreviousHealth} : {difference}");
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct DeadComponentTag : IComponentData { }
}
