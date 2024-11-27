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
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<EnemyPrefabEntityReference>(entity);

            for (int i = 0; i < authoring.enemyDataVariants.Length; i++)
            {
                var prefabEntity = GetEntity(authoring.enemyDataVariants[i].prefab, TransformUsageFlags.Dynamic);
                buffer.Add(new EnemyPrefabEntityReference { PrefabEntity = prefabEntity, Level = authoring.enemyDataVariants[0].level });
            }

        }
    }

}

public struct EnemyPrefabEntityReference : IBufferElementData
{
    public Entity PrefabEntity;
    public int Level;
}