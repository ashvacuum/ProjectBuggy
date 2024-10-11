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
            
            
            collisionCheckJob.Complete();
        }
    }

    public struct TriggerCheckJob : ITriggerEventsJob
    {
        
        public ComponentLookup<Projectile> ProjectileGroup;
        public void Execute(TriggerEvent triggerEvent)
        {
            
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
