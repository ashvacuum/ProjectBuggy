using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Enemy;
using Unity.Physics;
using UnityEditor.PackageManager;

public class EnemyDatabaseAuthoring : MonoBehaviour
{
    public EnemyVariantData[] enemyDataVariants;


    public class EnemyDatabaseBaker : Baker<EnemyDatabaseAuthoring>
    {
        public override void Bake(EnemyDatabaseAuthoring authoring)
        {

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
          
            for (int i = 0; i < authoring.enemyDataVariants.Length; i++)
            {
                //var entity = GetEntity(TransformUsageFlags.None);
                var entity = entityManager.CreateEntity();
                var prefabEntity = GetEntity(authoring.enemyDataVariants[i].prefab, TransformUsageFlags.Dynamic);
                if (prefabEntity == Entity.Null)
                {
                    Debug.LogError($"Failed to retrieve prefab entity for EnemyVariantData: {authoring.enemyDataVariants[i].name}");
                    continue;
                }
                entityManager.AddComponent<EnemyPrefabEntityReference>(entity);
                entityManager.SetComponentData(entity, new EnemyPrefabEntityReference
                {
                    PrefabEntity = prefabEntity,
                    Level = authoring.enemyDataVariants[i].level
                });
            }

        }
    }

}


public struct EnemyPrefabEntityReference : IComponentData
{
    public Entity PrefabEntity;
    public int Level;
}