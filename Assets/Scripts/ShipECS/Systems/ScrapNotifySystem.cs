using NonECS.UI;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct ScrapNotifySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScrapNotify>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (scrapData, entity) in SystemAPI.Query<RefRO<ScrapNotify>>().WithEntityAccess())
            {
                GameSceneEvents.Instance.UpdateExp(scrapData.ValueRO.ScrapValue);
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
        }
    }
}
