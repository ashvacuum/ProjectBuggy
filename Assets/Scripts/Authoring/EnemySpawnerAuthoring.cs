using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {

        public GameObject EnemyPrefab;
        public float Radius;
        public int MaximumEnemies;
        private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EnemySpawnerData()
                {
                    EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                    Radius = authoring.Radius,
                    MaximumEnemies = authoring.MaximumEnemies
                });
            }
        }
    }

    public struct EnemySpawnerData : IComponentData
    {
        public Entity EnemyPrefab;
        public float Radius;
        public int MaximumEnemies;
    }
}
