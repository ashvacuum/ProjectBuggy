using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    public struct SpatialGridData : IComponentData
    {
        public int2 GridCell;
    }

// Spatial grid singleton to track entities
    public struct SpatialGrid : IComponentData
    {
        public NativeParallelMultiHashMap<int2, Entity> CellToEntities;
        public float CellSize;
    }

// Add this system before your EnemyMovementSystem
    [UpdateInGroup(typeof(PostPhysicsPausableSystemGroup))]
    [UpdateBefore(typeof(EnemyMovementSystem))]
    public partial struct SpatialGridSystem : ISystem
    {
        private EntityQuery _enemyQuery;
    
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            // Create the spatial grid singleton
            state.EntityManager.AddComponentData(state.SystemHandle, new SpatialGrid
            {
                CellToEntities = new NativeParallelMultiHashMap<int2, Entity>(1000, Allocator.Persistent),
                CellSize = 5.0f // Adjust based on your game scale
            });
        
            // Setup enemy query
            _enemyQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<EnemyFollowTarget, LocalTransform>()
                .WithNone<DeadComponentTag>()
                .Build(state.EntityManager);
        }
    
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<SpatialGrid>())
            {
                var grid = SystemAPI.GetSingletonRW<SpatialGrid>();
                grid.ValueRW.CellToEntities.Dispose();
            }
        }
    
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get the ECB
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
            var grid = SystemAPI.GetSingletonRW<SpatialGrid>();
            float cellSize = grid.ValueRO.CellSize;
        
            // Clear the grid
            grid.ValueRW.CellToEntities.Clear();
        
            // Create a job to update grid positions
            var updateGridCellsJob = new UpdateGridCellsJob
            {
                CellToEntities = grid.ValueRW.CellToEntities.AsParallelWriter(),
                CellSize = cellSize,
                ECB = ecb.AsParallelWriter()
            };
        
            updateGridCellsJob.ScheduleParallel(_enemyQuery, state.Dependency).Complete();
        }
    
        [WithAll(typeof(EnemyFollowTarget))]
        [WithNone(typeof(DeadComponentTag))]
        [BurstCompile]
        private partial struct UpdateGridCellsJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter CellToEntities;
            public float CellSize;
            public EntityCommandBuffer.ParallelWriter ECB;
        
            public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, in LocalTransform transform)
            {
                // Calculate grid cell
                float3 position = transform.Position;
                int2 cell = new int2(
                    (int)math.floor(position.x / CellSize),
                    (int)math.floor(position.z / CellSize)
                );
            
                // Add to grid
                CellToEntities.Add(cell, entity);
            
                // Update or add the entity's grid cell data using ECB
                ECB.SetComponent(chunkIndex, entity, new SpatialGridData { GridCell = cell });
            }
        }
    }
}