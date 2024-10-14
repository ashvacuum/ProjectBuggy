using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
                DamageGroup = SystemAPI.GetComponentLookup<DamageComponent>(true),
                HealthGroup = SystemAPI.GetComponentLookup<HealthComponent>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
/*
            var triggerEventJob = new TriggerCheckJob()
            {
                ProjectileGroup = SystemAPI.GetComponentLookup<Projectile>(true),
                EnemyGroup = SystemAPI.GetComponentLookup<HealthComponent>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            
            triggerEventJob.Complete();
            */
            collisionCheckJob.Complete();
        }
    }

    public struct TriggerCheckJob : ITriggerEventsJob
    {
        
        public ComponentLookup<Projectile> ProjectileGroup;
        public ComponentLookup<HealthComponent> EnemyGroup;
        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (ProjectileGroup.HasComponent(entityA) && EnemyGroup.HasComponent(entityB) &&
                EnemyGroup.HasComponent(entityA))
            {
                
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

            if (HealthGroup.HasComponent(entityA) && DamageGroup.HasComponent(entityB))
            {
                var healthA = HealthGroup[entityA];
                var damageB = DamageGroup[entityB];

                if (!(healthA.CurrentNextTimeToTakeDamage <= 0)) return;

                healthA.PreviousHealth = healthA.CurrentHealth;
                healthA.CurrentHealth -= damageB.Damage;
                healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                //Debug.Log($"Entity A took {damageB.Damage}, total Health : {healthA.CurrentHealth}, {healthA.CurrentNextTimeToTakeDamage}");
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
                //Debug.Log($"Entity B took {damageA.Damage}, total Health : {healthB.CurrentHealth}, {healthB.CurrentNextTimeToTakeDamage }");
                HealthGroup[entityB] = healthB;


            }
        }
    }
}
