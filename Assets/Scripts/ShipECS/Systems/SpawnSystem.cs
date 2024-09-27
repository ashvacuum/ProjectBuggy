using Authoring;
using CameraSystem;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct SpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Spawner>();
            
        }

        public void OnUpdate(ref SystemState state)
        {
            

            var shipQuery = SystemAPI.QueryBuilder().WithAll<ShipComponent>().Build();
            if (shipQuery.IsEmpty)
            {
                var prefab = SystemAPI.GetSingleton<Spawner>().Prefab;
                var instances = state.EntityManager.Instantiate(prefab);
                var transform = SystemAPI.GetComponentRW<LocalTransform>(instances);
               // transform.ValueRW.Position = 
            }
        }
    }
    
}
