using NonECS.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct LevelUpNotifySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hasLvlBuffers = SystemAPI.TryGetSingletonBuffer<LevelUpBuffer>(out var lvlBuffers);
            if (!hasLvlBuffers) return;
            foreach (var lvlUp in lvlBuffers)
            {
                //TODO: create a way to show UI to use level up buffers
            }
            lvlBuffers.Clear();

        }
    }
}
