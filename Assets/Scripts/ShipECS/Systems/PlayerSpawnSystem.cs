using Authoring;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct PlayerSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            
            var shipQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag>().Build();
            if (!shipQuery.IsEmpty) return;
            
            var prefab = SystemAPI.GetSingleton<PlayerSpawner>().Prefab;
           
            state.EntityManager.Instantiate(prefab);
        }
    }
    
}
