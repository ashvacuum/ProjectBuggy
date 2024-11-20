using Authoring;
using Unity.Burst;
using Unity.Entities;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [BurstCompile]
    public partial struct GameTimerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
        
            foreach (var timer in SystemAPI.Query<RefRW<GameTimerComponent>>())
            {
                timer.ValueRW.TotalGameTime += deltaTime;
            }
        }

    }
}