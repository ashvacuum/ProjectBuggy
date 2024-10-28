using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    public partial struct LootSpawnSystem : ISystem
    {
        private EntityQuery lootTableQuery;

        public void OnCreate(ref SystemState state)
        {
            lootTableQuery = state.GetEntityQuery(ComponentType.ReadOnly<LootTableComponent>());
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Get the game timer
            var gameTime = 0f;
            foreach (var timer in SystemAPI.Query<RefRO<GameTimerComponent>>())
            {
                gameTime = timer.ValueRO.TotalGameTime;
                break;
            }

            // Process dead entities
            foreach (var (_, transform, entity) in 
                     SystemAPI.Query<RefRO<DeadComponentTag>, RefRO<LocalTransform>>()
                         .WithEntityAccess().WithNone<PlayerTag,ProjectileMotion>())
            {
                // Get loot table
                foreach (var lootTable in SystemAPI.Query<RefRO<LootTableComponent>>())
                {
                    ref var lootTableBlob = ref lootTable.ValueRO.LootTable.Value;
                
                    // Check each possible loot
                    for (var i = 0; i < lootTableBlob.PossibleLoots.Length; i++)
                    {
                        ref var lootEntry = ref lootTableBlob.PossibleLoots[i];
                        var currentChance = GetCurrentDropChance(gameTime, ref lootEntry);

                        if (!(UnityEngine.Random.Range(0f, 100f) <= currentChance)) continue;
                        // Spawn loot entity
                        var lootEntity = ecb.Instantiate(lootEntry.PrefabEntity);
                        ecb.SetComponent(lootEntity, LocalTransform.FromPosition(transform.ValueRO.Position));
                    }
                }

                // Destroy the dead entity
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }


        private float GetCurrentDropChance(float currentGameTime, ref LootEntryBlob lootEntry)
        {
            var highestApplicableChance = 0f;

            for (var i = 0; i < lootEntry.TimeThresholds.Length; i++)
            {
                if (currentGameTime >= lootEntry.TimeThresholds[i].TimeThreshold)
                {
                    highestApplicableChance = math.max(highestApplicableChance, 
                        lootEntry.TimeThresholds[i].DropChance);
                }
            }

            return highestApplicableChance;
        }
    }
}