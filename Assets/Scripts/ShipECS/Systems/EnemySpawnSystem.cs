using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ShipECS.Systems
{
    public partial struct EnemySpawnSystem : ISystem
    {
        uint seedOffset;
        float spawnTimer;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySpawnerData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            const float spawnWait = 0.05f; // 0.05 seconds

            spawnTimer -= SystemAPI.Time.DeltaTime;
            if (spawnTimer > 0)
            {
                return;
            }

            spawnTimer = spawnWait;

            var spawnData = SystemAPI.GetSingleton<EnemySpawnerData>();
            var count = spawnData.MaximumEnemies;
            
            // Remove the NewSpawn tag component from the entities spawned in the prior frame.
            var newSpawnQuery = SystemAPI.QueryBuilder().WithAll<NewEnemySpawn>().Build();
            state.EntityManager.RemoveComponent<NewEnemySpawn>(newSpawnQuery);
            
            var enemySpawnQuery = SystemAPI.QueryBuilder().WithAll<EnemyFollowTarget>().Build();
            var totalCount = 0;
            
            foreach (var (playerTransform, playerEntity) in
                     SystemAPI.Query<RefRO<EnemyFollowTarget>>()
                         .WithEntityAccess())
            {
                totalCount++;
            }

            if (totalCount >= count)
            {
                return;
            }
            
            // Spawn the enemies
            var prefab = spawnData.EnemyPrefab;
            state.EntityManager.Instantiate(prefab, count, Allocator.Temp);
            seedOffset += (uint)count;
            foreach (var (transform, entity) in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<ShipComponent>()
                         .WithEntityAccess())
            {
                new RandomPositionJob
                {
                    SeedOffset = seedOffset,
                    ReferencePoint = transform.ValueRO.Position,
                    MinRadius = spawnData.MinRadius,
                    MaxRadius = spawnData.MaxRadius,
                }.ScheduleParallel();
            }

            
        }
    }

    [WithAll(typeof(NewEnemySpawn))]
    [BurstCompile]
    partial struct RandomPositionJob : IJobEntity
    {
        public uint SeedOffset;
        public float MinRadius;
        public float MaxRadius;
        public float3 ReferencePoint;

        private void Execute([EntityIndexInQuery] int index, ref LocalTransform transform)
        {
            // Random instances with similar seeds produce similar results, so to get proper
            // randomness here, we use CreateFromIndex, which hashes the seed.
            var random = Random.CreateFromIndex(SeedOffset + (uint)index);
            var randomRadius = MinRadius + random.NextFloat() * (MaxRadius - MinRadius);
            var xz = random.NextFloat2Direction() * randomRadius;

            transform.Position = new float3(ReferencePoint.x + xz[0], 0, ReferencePoint.z + xz[1]);
        }
    }
}