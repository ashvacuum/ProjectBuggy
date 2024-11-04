using Unity.Entities;
using UnityEngine;
using Enemy;

public struct EnemyDataContainerBlob
{
    public Entity prefab;
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
