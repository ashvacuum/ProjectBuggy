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
        private SimulationSingleton Simulation;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            Simulation = SystemAPI.GetSingleton<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeReference<int> numCollisionEvents = new NativeReference<int>(0, Allocator.TempJob);

            var collisionEvents = Simulation.AsSimulation().CollisionEvents;
            /*
            new CollisionCheckJob()
            {
                
            }.Schedule(,Simulation,)
            
        */

            // ...
        }
    }

    public struct CollisionCheckJob : ICollisionEventsJob
    {
        public float DeltaTime;
        public ComponentLookup<HealthComponent> HealthGroup;
        // To read damage data
        [ReadOnly] public ComponentLookup<DamageComponent> DamageGroup;
        
        
        public void Execute(CollisionEvent collisionEvent)
        {
            
        }
    }
}
