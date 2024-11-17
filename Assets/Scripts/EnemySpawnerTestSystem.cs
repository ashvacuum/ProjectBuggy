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
        foreach (var enemyVariant in SystemAPI.Query<DynamicBuffer<EnemyPrefabEntityReference>>())
        {
            for (int i = 0; i < enemyVariant.Length; i++)
            {
                var instance = entityManager.Instantiate(enemyVariant[i].PrefabEntity, count, Allocator.Temp);

            }
        }


    }
}
