using Unity.Entities;
using Unity.Collections;
using UnityEngine;

// This struct holds the data for each enemy prefab, including its entity reference and level.
public struct EnemyData : IComponentData
{
    public Entity Prefab;
    public int Level;
}
[System.Serializable]
public class EnemyDataContainer
{
    public enum EnemyClass{
        Minion,
        Tower,
        Boss,
       
    }
    public GameObject prefabs;
    public int level;
    public EnemyClass enemyClass;
}
// This is the authoring component that allows you to assign enemy prefabs and level in the inspector.
public class EnemyDatabaseAuthoring : MonoBehaviour
{
    /*public GameObject[] prefabs;
    public int[] level;*/

    public EnemyDataContainer[] enemy;
}

// This baker converts the authoring component into ECS-friendly data.
public class EnemyDatabaseBaker : Baker<EnemyDatabaseAuthoring>
{
    public override void Bake(EnemyDatabaseAuthoring authoring)
    {


        // Create a NativeList to hold the converted enemy data.
        var enemyDataList = new NativeList<EnemyData>(authoring.enemy.Length, Allocator.Temp);

        // Convert each prefab into an entity and store it with the level.
        for (int i = 0; i < authoring.enemy.Length; i++)
        {
            Entity prefabEntity = GetEntity(authoring.enemy[i].prefabs, TransformUsageFlags.Renderable);
            enemyDataList.Add(new EnemyData
            {
                Prefab = prefabEntity,
                Level = authoring.enemy[i].level
            });
        }

        // Create a DynamicBuffer and add each EnemyData to it.
        var buffer = AddBuffer<EnemyDataBuffer>(GetEntity());
        foreach (var enemyData in enemyDataList)
        {
            buffer.Add(new EnemyDataBuffer
            {
                Prefab = enemyData.Prefab,
                Level = enemyData.Level
            });
        }

        enemyDataList.Dispose();
    }
}

// Buffer element to store multiple EnemyData in a dynamic buffer.
public struct EnemyDataBuffer : IBufferElementData
{
    public Entity Prefab;
    public int Level;
}
