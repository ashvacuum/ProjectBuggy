using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            var currentEnemyCount = enemySpawnQuery.ToEntityArray(Allocator.Temp).Length;

            if (currentEnemyCount > count)
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
                    Radius = spawnData.Radius
                }.ScheduleParallel();
            }

            
        }
    }

    [WithAll(typeof(NewEnemySpawn))]
    [BurstCompile]
    partial struct RandomPositionJob : IJobEntity
    {
        public uint SeedOffset;
        public float Radius;
        public float3 ReferencePoint;

        public void Execute([EntityIndexInQuery] int index, ref LocalTransform transform)
        {
            // Random instances with similar seeds produce similar results, so to get proper
            // randomness here, we use CreateFromIndex, which hashes the seed.
            var random = Random.CreateFromIndex(SeedOffset + (uint)index);
            var xz = random.NextFloat2Direction() * Radius;
            transform.Position = new float3(ReferencePoint.x + xz[0], 50,ReferencePoint.z + xz[1]);
        }
    }
}