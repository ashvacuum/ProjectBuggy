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

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyDataComponent>();
        state.RequireForUpdate<EnemyDataMap>();

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
        ref var blobAssetHashMap = ref enemyDataMap.EnemyDataMapRef.Value;
        if (blobAssetHashMap.TryGetValue(new FixedString32Bytes(blobArray[0].enemyID.ToString()), out Entity enemyVarietyEntity))
        {
            //Debug.Log($"Entity associated with 'Player': {enemyVarietyEntity}");
            state.EntityManager.Instantiate(enemyVarietyEntity, count, Allocator.Temp); 
        }
        else
        {

        }



    }
}
