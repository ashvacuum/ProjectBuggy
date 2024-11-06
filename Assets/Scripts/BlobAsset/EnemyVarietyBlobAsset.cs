using Unity.Entities;
using UnityEngine;
using Enemy;

public struct EnemyDataContainerBlob
{
    //public Entity enemyPrefab;
    public EnemyPrefabID enemyID;
    public int level;
    public EnemyClass enemyClass;
}
public struct EnemyVarietyBlobAsset 
{
/*    public Entity prefab;
    public int level;
    public EnemyClass enemyClass;*/

    public BlobArray<EnemyDataContainerBlob> enemyArray;
}
