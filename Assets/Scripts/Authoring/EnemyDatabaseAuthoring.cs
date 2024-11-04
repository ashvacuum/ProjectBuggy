using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Enemy;
using Unity.Physics;
using UnityEditor.PackageManager;


// This is the authoring component that allows you to assign enemy prefabs and level in the inspector.
public class EnemyDatabaseAuthoring : MonoBehaviour
{
    public EnemyVariantData[] enemyVariants;
}

// This baker converts the authoring component into ECS-friendly data.
public class EnemyDatabaseBaker : Baker<EnemyDatabaseAuthoring>
{
    public BlobAssetReference<EnemyVarietyBlobAsset> CreateBlobAsset(EnemyVariantData[] initialData)
    {
        using (var builder = new BlobBuilder(Allocator.Temp))
        {
            ref var root = ref builder.ConstructRoot<EnemyVarietyBlobAsset>();
            var array = builder.Allocate(ref root.enemyArray, initialData.Length);
            for (int i = 0; i < initialData.Length; i++)
            {
                array[i].prefab = GetEntity(initialData[i].enemy.prefab, TransformUsageFlags.Dynamic);
                array[i].level = initialData[i].enemy.level;
                array[i].enemyClass = initialData[i].enemy.enemyClass;
            }
            return builder.CreateBlobAssetReference<EnemyVarietyBlobAsset>(Allocator.Persistent);
        }
    }


    public override void Bake(EnemyDatabaseAuthoring authoring)
    {


        var entity = GetEntity(TransformUsageFlags.None);
        var blobAsset = CreateBlobAsset(authoring.enemyVariants);
        AddComponent(entity, new EnemyDataComponent { EnemyVarietyBlob = blobAsset });
        
        blobAsset.Dispose();
    }
}


public struct EnemyDataComponent : IComponentData
{
    public BlobAssetReference<EnemyVarietyBlobAsset> EnemyVarietyBlob;
}

