using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ShipECS.Systems
{
    internal partial struct DeadComponentCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DeadComponentTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (deadTag, entity) in SystemAPI.Query<RefRW<DeadComponentTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }

        
    }
}
