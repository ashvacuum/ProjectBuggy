using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct DamageCollisionSystem : ISystem
    {
        private ComponentLookup<DamageComponent> Damage;
        private ComponentLookup<HealthComponent> Health;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            Damage = state.GetComponentLookup<DamageComponent>(true);
            Health = state.GetComponentLookup<HealthComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Damage.Update(ref state);
            Health.Update(ref state);
            
            foreach (var (triggerEventBuffer, entity) in SystemAPI
                         .Query<DynamicBuffer<StatefulTriggerEvent>>().WithEntityAccess())
            {
                foreach (var trigger in triggerEventBuffer)
                {
                    var currentTriggerEventBuffer = trigger;
                    var entityB = currentTriggerEventBuffer.GetOtherEntity(entity);
                    if (trigger.State != StatefulEventState.Enter) continue;
                    var healthB = Health[entityB];
                    var damageComponent = Damage[entity];
                    if (healthB.CurrentHealth <= 0 || !(healthB.CurrentNextTimeToTakeDamage <= 0)) return;
                    healthB.PreviousHealth = healthB.CurrentHealth;
                    healthB.CurrentHealth -= damageComponent.Damage;
                    healthB.CurrentNextTimeToTakeDamage = healthB.NextTimeToTakeDamage;
                    //Debug.Log($"Entity B took Trigger {damageComponent.Damage}, total Health : {healthB.CurrentHealth}, {healthB.CurrentNextTimeToTakeDamage }");
                    Health[entityB] = healthB;
                }
            }
            
            foreach (var (triggerEventBuffer, entity) in SystemAPI
                         .Query<DynamicBuffer<StatefulCollisionEvent>>().WithEntityAccess())
            {
                foreach (var trigger in triggerEventBuffer)
                {
                    var currentTriggerEventBuffer = trigger;
                    var entityB = currentTriggerEventBuffer.GetOtherEntity(entity);
                    if (trigger.State == StatefulEventState.Exit) continue;
                    
                    var healthA = Health[entity];
                    var damageComponent = Damage[entityB];
                    if (healthA.CurrentHealth <= 0 || !(healthA.CurrentNextTimeToTakeDamage <= 0)) return;
                    healthA.PreviousHealth = healthA.CurrentHealth;
                    healthA.CurrentHealth -= damageComponent.Damage;
                    healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                    Debug.Log($"Entity A took Collision {damageComponent.Damage}, total Health : {healthA.CurrentHealth}, {healthA.CurrentNextTimeToTakeDamage }");
                    Health[entity] = healthA;
                }
            }
        }
    }

}
