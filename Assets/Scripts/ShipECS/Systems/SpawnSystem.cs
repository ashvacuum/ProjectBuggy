using Authoring;
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
            if (!shipQuery.IsEmpty) return;
            var prefab = SystemAPI.GetSingleton<Spawner>().Prefab;
            state.EntityManager.Instantiate(prefab);
        }
    }
    
}
