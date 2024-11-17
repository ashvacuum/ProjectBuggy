using Authoring;
using Enemy;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Core;


public partial struct EnemySpawnerTestSystem : ISystem
{
    private EntityQuery m_PrefabEntityReferenceQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyPrefabEntityReference>();
        m_PrefabEntityReferenceQuery = state.GetEntityQuery(typeof(EnemyPrefabEntityReference));
    }

    public void OnUpdate(ref SystemState state)
    {
        var count = 5;

        //Max Count
        var totalCount = 0;
        foreach (var playerTransform in
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<EnemyFollowTarget>())
        {
            totalCount++;
        }
        if (totalCount >= count) return;
        count -= totalCount;


     EntityManager entityManager = state.EntityManager;
        NativeArray<Entity> entities = m_PrefabEntityReferenceQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < entities.Length; i++)
        {
            var spawnData = entityManager.GetComponentData<EnemyPrefabEntityReference>(entities[i]);
            Debug.Log("Enemy: " + i+" " + spawnData.PrefabEntity);
            Debug.Log("Enemy: " + i+" Level: " + spawnData.Level);

            Entity prefab = spawnData.PrefabEntity;
            
            var instance = entityManager.Instantiate(spawnData.PrefabEntity, count, Allocator.Persistent);
        }

        /*foreach (var spawner in SystemAPI.Query<RefRW<EnemyPrefabEntityReference>>())
        {
            var spawnData = spawner.ValueRW;
            //timer interval
            var instance = entityManager.Instantiate(spawnData.PrefabEntity, count, Allocator.Temp);


        }*/

    }
}
