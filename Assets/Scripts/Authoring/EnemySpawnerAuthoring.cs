using System;
using System.Collections.Generic;
using Unity.Collections;
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
        public float MaximumTime = 1800;
        public float BossTime = 300f;
        
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
                    MaximumEnemies = authoring.MaximumEnemies,
                    MaximumTime = authoring.MaximumTime,
                    BossTime = authoring.BossTime
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
        public float MaximumTime;
        public float BossTime;
    }
}
