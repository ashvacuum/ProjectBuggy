using NonECS.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct LevelUpNotifySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManagerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hasLvlBuffers = SystemAPI.TryGetSingletonBuffer<LevelUpBuffer>(out var lvlBuffers);
            
            if (!hasLvlBuffers) return;
            
            var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();

            
            pauseState.IsPaused = lvlBuffers.Length > 0;
            
            SystemAPI.SetSingleton(pauseState);
            
            foreach (var lvlUp in lvlBuffers)
            {
                //Display UI here or maybe elsewhere
            }
            lvlBuffers.Clear();

        }
    }
}
