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

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeReference<int> numCollisionEvents = new NativeReference<int>(0, Allocator.TempJob);

            var collisionEvents = SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation().CollisionEvents;
            var collisionCount = 0;
            foreach (var collisionEvent in collisionEvents)
            {
                collisionCount++;
            }
            
            Debug.Log($"{collisionCount}");
        

            // ...
        }
    }
}
