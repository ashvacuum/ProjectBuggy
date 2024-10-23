using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Content;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct DamageCollisionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionCheckJob  = new CollisionCheckJob()
            {
                DamageGroup = state.GetComponentLookup<DamageComponent>(true),
                HealthGroup = state.GetComponentLookup<HealthComponent>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            var triggerEventJob = new TriggerCheckJob()
            {
                DamageGroup = state.GetComponentLookup<DamageComponent>(true),
                EnemyGroup = state.GetComponentLookup<HealthComponent>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), collisionCheckJob);
            triggerEventJob.Complete();
            
            
            collisionCheckJob.Complete();
            
        }
    }

    public struct TriggerCheckJob : ITriggerEventsJob
    {
        
        [ReadOnly] public ComponentLookup<DamageComponent> DamageGroup;
        public ComponentLookup<HealthComponent> EnemyGroup;
        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
            if (entityA == Entity.Null || entityB == Entity.Null) return;
            if (DamageGroup.HasComponent(entityA) && EnemyGroup.HasComponent(entityB))
            {
                var healthB = EnemyGroup[entityB];
                var damageA = DamageGroup[entityA];
                if (healthB.CurrentHealth <= 0 || !(healthB.CurrentNextTimeToTakeDamage <= 0)) return;
                
                healthB.PreviousHealth = healthB.CurrentHealth;
                healthB.CurrentHealth -= damageA.Damage;
                healthB.CurrentNextTimeToTakeDamage = healthB.NextTimeToTakeDamage;
                Debug.Log($"Entity B took Trigger {damageA.Damage}, total Health : {healthB.CurrentHealth}, {healthB.CurrentNextTimeToTakeDamage }");
                EnemyGroup[entityB] = healthB;
            }

            if (EnemyGroup.IsComponentEnabled(entityA) && DamageGroup.HasComponent(entityB) && EnemyGroup.HasComponent(entityA))
            {
                var healthA = EnemyGroup[entityA];
                var damageB = DamageGroup[entityB];

                if (healthA.CurrentHealth <= 0 &&  !(healthA.CurrentNextTimeToTakeDamage <= 0)) return;

                healthA.PreviousHealth = healthA.CurrentHealth;
                healthA.CurrentHealth -= damageB.Damage;
                healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                Debug.Log($"Entity A took Trigger {damageB.Damage}, total Health : {healthA.CurrentHealth}, {healthA.CurrentNextTimeToTakeDamage}");
                EnemyGroup[entityA] = healthA;
            }
        }
    }

    [BurstCompile]
    public struct CollisionCheckJob : ICollisionEventsJob
    {
        public ComponentLookup<HealthComponent> HealthGroup;
        // To read damage data
        [ReadOnly] public ComponentLookup<DamageComponent> DamageGroup;
        
        
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;
            if (entityA == Entity.Null || entityB == Entity.Null) return;
            if (HealthGroup.HasComponent(entityA) && DamageGroup.HasComponent(entityB))
            {
                var healthA = HealthGroup[entityA];
                var damageB = DamageGroup[entityB];

                if (!(healthA.CurrentNextTimeToTakeDamage <= 0)) return;

                healthA.PreviousHealth = healthA.CurrentHealth;
                healthA.CurrentHealth -= damageB.Damage;
                healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                Debug.Log($"Entity A took Collision {damageB.Damage}, total Health : {healthA.CurrentHealth}, {healthA.CurrentNextTimeToTakeDamage}");
                HealthGroup[entityA] = healthA;
            }

            if (HealthGroup.HasComponent(entityB) && DamageGroup.HasComponent(entityA))
            {
                var healthB = HealthGroup[entityB];
                var damageA = DamageGroup[entityA];
                
                if (!(healthB.CurrentNextTimeToTakeDamage <= 0)) return;
                
                healthB.PreviousHealth = healthB.CurrentHealth;
                healthB.CurrentHealth -= damageA.Damage;
                healthB.CurrentNextTimeToTakeDamage = healthB.NextTimeToTakeDamage;
                Debug.Log($"Entity B took Collision {damageA.Damage}, total Health : {healthB.CurrentHealth}, {healthB.CurrentNextTimeToTakeDamage }");
                HealthGroup[entityB] = healthB;


            }
        }
    }
}
