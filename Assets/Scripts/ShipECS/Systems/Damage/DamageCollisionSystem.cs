using Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Stateful;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ShipECS.Systems.Damage
{
    public class DamageConstants
    {
        public const float CritMultiplier = 1.5f;
    }
    [UpdateInGroup(typeof(PostPhysicsPausableSystemGroup))]
    public partial struct DamageCollisionSystem : ISystem
    {
        private ComponentLookup<DamageComponent> _damage;
        private ComponentLookup<HealthComponent> _health;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            _damage = state.GetComponentLookup<DamageComponent>(true);
            _health = state.GetComponentLookup<HealthComponent>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            _damage.Update(ref state);
            _health.Update(ref state);

            foreach (var (triggerEventBuffer, entityA) in SystemAPI 
                         .Query<DynamicBuffer<StatefulTriggerEvent>>().WithEntityAccess())
            {
                foreach (var trigger in triggerEventBuffer)
                {
                    var currentTriggerEventBuffer = trigger;
                    var entityB = currentTriggerEventBuffer.GetOtherEntity(entityA);
                    
                    if (trigger.State != StatefulEventState.Enter) continue;

                    if (!_health.HasComponent(entityA) || !_damage.HasComponent(entityB) || !_health.HasComponent(entityB) || !_damage.HasComponent(entityA)) continue;
                    
                    var healthB = _health[entityB];
                    var damageComponent = _damage[entityA];
                    var healthA = _health[entityA];

                    var random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime * 1000);
                    if (healthB.CurrentHealth <= 0 || !(healthB.CurrentNextTimeToTakeDamage <= 0)) return;
                    healthB.PreviousHealth = healthB.CurrentHealth;
                    healthB.CurrentHealth -= damageComponent.Damage;
                    var rolledChance = random.NextFloat(0, 100);
                    
                    if (damageComponent.CriticalChance > 0 && rolledChance < damageComponent.CriticalChance)
                    {
                        Debug.Log($"Rolled {rolledChance}/{damageComponent.CriticalChance} ");
                        healthB.CurrentHealth -= damageComponent.Damage * DamageConstants.CritMultiplier;
                        healthB.WasDamagedCritical = true;
                    }
                    else
                    {
                        healthB.CurrentHealth -= damageComponent.Damage;
                        healthB.WasDamagedCritical = false;
                    }
                    
                    healthB.CurrentNextTimeToTakeDamage = healthB.NextTimeToTakeDamage;
                    _health[entityB] = healthB;
                    
                    healthA.CurrentHealth -= 1;
                    healthA.PreviousHealth =
                        healthA.CurrentHealth; //prevents damage system from computing any damage

                    healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                    _health[entityA] = healthA;



                }
            }
            
            foreach (var (triggerEventBuffer, entityA) in SystemAPI
                         .Query<DynamicBuffer<StatefulCollisionEvent>>().WithEntityAccess())
            {
                foreach (var trigger in triggerEventBuffer)
                {
                    var currentTriggerEventBuffer = trigger;
                    var entityB = currentTriggerEventBuffer.GetOtherEntity(entityA);
                    if (trigger.State == StatefulEventState.Exit) continue;
                    
                    
                    if (!_health.HasComponent(entityA) || !_damage.HasComponent(entityB)) continue;
                    var healthA = _health[entityA];
                    var damageComponent = _damage[entityB];
                    if (healthA.CurrentHealth <= 0 || !(healthA.CurrentNextTimeToTakeDamage <= 0)) return;
                    healthA.PreviousHealth = healthA.CurrentHealth;
                    healthA.CurrentHealth -= damageComponent.Damage;
                    healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                    _health[entityA] = healthA;
                }
            }
            
        }
    }

}
