using Unity.Burst;
using Unity.Entities;

namespace ShipECS.Systems
{
    //do not include this in the pausable system group
    //[UpdateInGroup(typeof(SimulationSystemGroup))]
    //[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    partial struct GamePauseSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManagerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();

            // Example of toggling pause with spacebar
            if (!UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space)) return;
            
            pauseState.IsPaused = !pauseState.IsPaused;
            SystemAPI.SetSingleton(pauseState);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
}
