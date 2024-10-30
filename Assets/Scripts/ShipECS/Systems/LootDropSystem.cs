using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct LootSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // Get the game timer
            var gameTime = 0f;
            foreach (var timer in SystemAPI.Query<RefRO<GameTimerComponent>>())
            {
                gameTime = timer.ValueRO.TotalGameTime;
                break;
            }
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            // Process entities with dead tag
            foreach (var (_, transform, entity) in 
                     SystemAPI.Query<RefRO<DeadComponentTag>, RefRO<LocalTransform>>()
                         .WithEntityAccess().WithNone<PlayerTag,ProjectileMotion>())
            {
                // Get loot table
                foreach (var lootTable in SystemAPI.Query<DynamicBuffer<LootTableAuthoring.LootDropTable>>())
                {
                    // Check each possible loot
                    for (var i = 0; i < lootTable.Length; i++)
                    {
                        var timeToSpawn = lootTable[i].timeThreshold;
                        var currentChance = lootTable[i].dropChance;
                        if (gameTime/60 <= timeToSpawn) continue;

                        var rolledChance = UnityEngine.Random.Range(0f, 100f);
                        if (!(rolledChance <= currentChance))
                        {
                            Debug.Log($"Failed Chance: {rolledChance} <= {currentChance}");
                            continue;
                        }
                        // Spawn loot entity
                        var lootEntity = ecb.Instantiate(lootTable[i].prefab);
                        ecb.SetComponent(lootEntity, LocalTransform.FromPosition(transform.ValueRO.Position));
                        Debug.Log($"Success Chance: {rolledChance} <= {currentChance}");
                        break;
                    }
                }

                // Destroy the dead entity
                //ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
    }
}