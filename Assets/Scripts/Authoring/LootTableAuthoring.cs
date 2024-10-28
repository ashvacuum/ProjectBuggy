using System;
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

        [Serializable]
        public struct LootEntryData
        {
            public GameObject prefab;
            public LootTimeThresholdData[] timeThresholds;
        }

        public LootEntryData[] possibleLoots;

        public class LootTableBaker : Baker<LootTableAuthoring>
        {
            public override void Bake(LootTableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
            
                // Create blob builder

                BlobAssetReference<LootTableBlob> lootTableBlob;
                using (var builder = new BlobBuilder(Allocator.Temp))
                {
                    ref var root = ref builder.ConstructRoot<LootTableBlob>();

                    // Allocate array for all possible loots
                    var lootArray = builder.Allocate(ref root.PossibleLoots, authoring.possibleLoots.Length);

                    // Fill in the data
                    for (var i = 0; i < authoring.possibleLoots.Length; i++)
                    {
                        var lootEntry = authoring.possibleLoots[i];
                        var thresholds = builder.Allocate(ref lootArray[i].TimeThresholds,
                            lootEntry.timeThresholds.Length);

                        // Convert GameObject prefab to Entity
                        var prefabEntity = GetEntity(lootEntry.prefab, TransformUsageFlags.Dynamic);
                        lootArray[i].PrefabEntity = prefabEntity;

                        // Fill in thresholds
                        for (var j = 0; j < lootEntry.timeThresholds.Length; j++)
                        {
                            thresholds[j] = new LootTimeThresholdBlob
                            {
                                TimeThreshold = lootEntry.timeThresholds[j].timeThreshold * 60f, // Convert to seconds
                                DropChance = lootEntry.timeThresholds[j].dropChance
                            };
                        }
                    }

                    lootTableBlob = builder.CreateBlobAssetReference<LootTableBlob>(Allocator.Persistent);
                }

                // Add component to entity
                AddBlobAsset(ref lootTableBlob, out var hash);
                AddComponent(entity, new LootTableComponent { LootTable = lootTableBlob });
            }
        }
    }

    public struct GameTimerComponent : IComponentData
    {
        public float TotalGameTime;
    }

    public struct LootTableComponent : IComponentData
    {
        public BlobAssetReference<LootTableBlob> LootTable;
    }
}