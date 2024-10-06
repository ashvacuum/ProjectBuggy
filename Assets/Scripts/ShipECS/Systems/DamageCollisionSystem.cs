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
    [UpdateBefore(typeof(PhysicsSimulationGroup))]
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
                DeltaTime = SystemAPI.Time.DeltaTime,
                DamageGroup = SystemAPI.GetComponentLookup<DamageComponent>(true),
                HealthGroup = SystemAPI.GetComponentLookup<HealthComponent>()
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            
            
            collisionCheckJob.Complete();
        }
    }

    [BurstCompile]
    public struct CollisionCheckJob : ICollisionEventsJob
    {
        public float DeltaTime;
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
                if (healthA.CurrentNextTimeToTakeDamage >= 0)
                {
                    healthA.Health -= damageB.Damage;
                    healthA.CurrentNextTimeToTakeDamage = healthA.NextTimeToTakeDamage;
                    Debug.Log($"Entity B took {damageB.Damage}, total Health : {healthA.Health}, {healthA.CurrentNextTimeToTakeDamage }");
                }
                else
                {
                    healthA.CurrentNextTimeToTakeDamage -= DeltaTime;
                    Debug.Log($"Entity A can't be damaged until {healthA.CurrentNextTimeToTakeDamage} seconds");
                }
                
                HealthGroup[entityA] = healthA;
                
               
            }
            
            if (HealthGroup.HasComponent(entityB) && DamageGroup.HasComponent(entityA))
            {
                var healthB = HealthGroup[entityB];
                var damageA = DamageGroup[entityA];
                if (healthB.CurrentNextTimeToTakeDamage >= 0)
                {
                    healthB.Health -= damageA.Damage;
                    healthB.CurrentNextTimeToTakeDamage = healthB.NextTimeToTakeDamage;
                    
                    Debug.Log($"Entity A took {damageA.Damage}, total Health : {healthB.Health}");
                }
                else
                {
                    healthB.CurrentNextTimeToTakeDamage -= DeltaTime;
                    Debug.Log($"Entity B can't be damaged until {healthB.CurrentNextTimeToTakeDamage} seconds");
                }
                HealthGroup[entityB] = healthB;
            }
        }
    }
}
