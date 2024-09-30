using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {

        public GameObject EnemyPrefab;
        [FormerlySerializedAs("Radius")] public float MinRadius;
        public float MaxRadius;
        public int MaximumEnemies;
        private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EnemySpawnerData()
                {
                    EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                    MinRadius = authoring.MinRadius,
                    MaxRadius = authoring.MaxRadius,
                    MaximumEnemies = authoring.MaximumEnemies
                });
            }
        }
    }

    public struct EnemySpawnerData : IComponentData
    {
        public Entity EnemyPrefab;
        public float MinRadius;
        public float MaxRadius;
        public int MaximumEnemies;
    }
}
