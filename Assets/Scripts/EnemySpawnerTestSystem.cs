using Authoring;
using Enemy;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct EnemySpawnerTestSystem : ISystem
{

    private EnemyPrefabMapping _prefabMapping;
    private EntityManager _entityManager;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyDatabaseAuthoring>();
        state.RequireForUpdate<EnemyDataMap>();

        //_prefabMapping = Resources.Load<EnemyPrefabMapping>("EnemyPrefabMapping");


    }
    public void OnUpdate(ref SystemState state)
    {
        var count = 5;

        var spawnData = SystemAPI.GetSingleton<EnemyDataComponent>();
        var enemyDataMap = SystemAPI.GetSingleton<EnemyDataMap>();
        
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



        //Spawner
        ref var blobAsset = ref spawnData.EnemyVarietyBlob.Value;
        ref var blobArray = ref blobAsset.enemyArray;
        ref var parallelHashMap = ref enemyDataMap.NPHMEnemyDataMap;
        if (parallelHashMap.TryGetValue(new FixedString32Bytes(blobArray[0].enemyID.ToString()), out Entity enemyVarietyEntity))
        {
            Debug.Log($"Entity associated with 'Player': {enemyVarietyEntity}");
            state.EntityManager.Instantiate(enemyVarietyEntity, count, Allocator.Temp);
        }
        else
        {
            Debug.Log("Key 'Player' not found.");
        }



    }
}
