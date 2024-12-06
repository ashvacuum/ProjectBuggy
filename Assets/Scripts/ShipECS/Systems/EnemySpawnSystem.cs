using Authoring;
using NonECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct EnemySpawnSystem : ISystem
    {
        uint seedOffset;
        float spawnTimer;
        private float totalTime;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EnemySpawnerData>();
            totalTime = 0;
        }

        public void OnUpdate(ref SystemState state)
        {

            const float spawnWait = 0.05f; // 0.05 seconds

            spawnTimer -= SystemAPI.Time.DeltaTime;
            totalTime += SystemAPI.Time.DeltaTime;
            if (spawnTimer > 0)
            {
                return;
            }

            spawnTimer = spawnWait;

            var spawnData = SystemAPI.GetSingleton<EnemySpawnerData>();
            var count = spawnData.MaximumEnemies;

            if (EnemySpawnSingleton.Instance != null)
            {
                //Debug.Log($"Total Time = {totalTime}");
                //TODO: figure out a different way of doing this, maybe scriptable objects?
                count = EnemySpawnSingleton.Instance.GetNumEnemiesBasedOnTime(totalTime);
            }

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Remove the NewSpawn and DisableRendering component from the entities spawned in the prior frame.

            foreach (var (spawn, children, entity) in
                     SystemAPI.Query<RefRO<NewEnemySpawn>, DynamicBuffer<Child>>()
                         .WithEntityAccess())
            {
                // Change layer for the root entity
                if (state.EntityManager.HasComponent<RenderFilterSettings>(entity))
                {
                    var renderSettings = state.EntityManager.GetSharedComponent<RenderFilterSettings>(entity);
                    renderSettings.Layer = 0; // Hardcoded to layer 0, adjust as needed
                    ecb.SetSharedComponent(entity, renderSettings);
                }

                // Recursively change layers for all nested children
                ChangeLayerRecursivelyDeep(ref state, ecb, entity, 0);
                

                // Remove the NewEnemySpawn tag
                ecb.RemoveComponent<NewEnemySpawn>(entity);
            }
            
            var totalCount = 0;

            foreach (var playerTransform in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<EnemyFollowTarget>())
            {
                totalCount++;
            }

            if (totalCount >= count) return;

            count -= totalCount;

            var prefab = spawnData.EnemyPrefab;
            state.EntityManager.Instantiate(prefab, count, Allocator.Temp);
            seedOffset += (uint)totalCount;

            foreach (var transform in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>())
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

        private void ChangeLayerRecursivelyDeep(ref SystemState state, EntityCommandBuffer ecb, Entity currentEntity, int newRenderLayer)
        {
            if (!state.EntityManager.HasBuffer<Child>(currentEntity)) return;
            var children = state.EntityManager.GetBuffer<Child>(currentEntity);
            for (int i = 0; i < children.Length; i++)
            {
                Entity childEntity = children[i].Value;

                // Change render layer for this child
                if (state.EntityManager.HasComponent<RenderFilterSettings>(childEntity))
                {
                    var renderSettings = state.EntityManager.GetSharedComponent<RenderFilterSettings>(childEntity);
                    renderSettings.Layer = newRenderLayer;
                    ecb.SetSharedComponent(childEntity, renderSettings);
                }

                // Recursively process THIS child's children (key difference)
                // This ensures we go as deep as the hierarchy goes
                ChangeLayerRecursivelyDeep(ref state, ecb, childEntity, newRenderLayer);
            }
        }
    }

    [WithAll(typeof(NewEnemySpawn))]
    [WithNone(typeof(DeadComponentTag))]
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
            //Debug.Log($"Transferring Position to : {transform.Position.x}, {transform.Position.z}");
        }
    }
}