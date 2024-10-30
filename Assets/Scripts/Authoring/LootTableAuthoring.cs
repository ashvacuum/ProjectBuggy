using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    // Blob data structures
    public struct LootTimeThresholdBlob
    {
        public float TimeThreshold;
        public float DropChance;
    }

    public struct LootEntryBlob
    {
        public BlobArray<LootTimeThresholdBlob> TimeThresholds;
        public Entity PrefabEntity;
    }

    public struct LootTableBlob
    {
        public BlobArray<LootEntryBlob> PossibleLoots;
    }

// Authoring Component
    public class LootTableAuthoring : MonoBehaviour
    {
        [Serializable]
        public struct LootTimeThresholdData
        {
            public float timeThreshold;
            public float dropChance;
        }
        
        public struct LootDropTable : IBufferElementData
        {
            public Entity prefab;
            public float timeThreshold;
            public float dropChance; 
        }
        
        [Serializable]
        public struct LootTableAuthoringData
        {
            public GameObject prefab;
            public LootTimeThresholdData[] timeThresholds;
        }

        public List<LootTableAuthoringData> possibleLoots;

        public class LootTableBaker : Baker<LootTableAuthoring>
        {
            public override void Bake(LootTableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var lootDropTables = AddBuffer<LootDropTable>(entity);
                foreach (var lootEntity in authoring.possibleLoots)
                {
                    var loot = GetEntity(lootEntity.prefab, TransformUsageFlags.Dynamic);
                    foreach (var timeThresholds in lootEntity.timeThresholds)
                    {
                        lootDropTables.Add(new LootDropTable()
                        {
                            dropChance = timeThresholds.dropChance,
                            timeThreshold = timeThresholds.timeThreshold,
                            prefab = loot
                        });
                    }
                   
                }
                AddComponent<GameTimerComponent>(entity);
            }
        }
    }

    public struct GameTimerComponent : IComponentData
    {
        public float TotalGameTime;
    }
}