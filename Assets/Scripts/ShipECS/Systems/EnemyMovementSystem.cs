using System;
using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PostPhysicsPausableSystemGroup))]
    [UpdateAfter(typeof(KnockBackSystem))]
    public partial struct EnemyMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpatialGrid>();
            state.RequireForUpdate<EnemyFollowTarget>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var grid = SystemAPI.GetSingleton<SpatialGrid>();
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);
           
            
            var neighborOffsets = new NativeArray<int2>(9, Allocator.TempJob);
            neighborOffsets[0] = new int2(-1, -1);
            neighborOffsets[1] = new int2(-1, 0);
            neighborOffsets[2] = new int2(-1, 1);
            neighborOffsets[3] = new int2(0, -1);
            neighborOffsets[4] = new int2(0, 0);
            neighborOffsets[5] = new int2(0, 1);
            neighborOffsets[6] = new int2(1, -1);
            neighborOffsets[7] = new int2(1, 0);
            neighborOffsets[8] = new int2(1, 1);
            foreach (var (playerTransform, playerEntity) in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                new FollowPlayerJob()
                {
                    TargetLocation = playerTransform.ValueRO.Position,
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    CellToEntities = grid.CellToEntities,
                    CellSize = grid.CellSize,
                    TransformLookup = transformLookup,
                    neighborOffsets = neighborOffsets
                }.Schedule();
            }
        }
    }
    
    [WithAll(typeof(EnemyFollowTarget), typeof(KnockBackReceiver), typeof(SpatialGridData))]
    [WithNone(typeof(NewEnemySpawn), typeof(DeadComponentTag))]
    [BurstCompile]
    public partial struct FollowPlayerJob : IJobEntity
    {
        public float3 TargetLocation;
        public float DeltaTime;
        [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> CellToEntities;
        public float CellSize;
        [ReadOnly] public NativeArray<int2> neighborOffsets;
        public ComponentLookup<LocalTransform> TransformLookup;
        
        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, 
            ref KnockBackReceiver receiver, in EnemyFollowTarget followTarget, 
            in PhysicsMass mass, in SpatialGridData gridData)
        {
            
            var shipTransform = TransformLookup[entity];
            if (receiver.isBeingKnockedBack)
            {
                var isKinematic = mass.IsKinematic;
                if (receiver is { isBeingKnockedBack: true } && isKinematic)
                {

                    var postTransform = new LocalTransform
                    {
                        Position = shipTransform.Position + receiver.currentKnockbackVelocity * DeltaTime,
                        Rotation = shipTransform.Rotation, //same rotation
                        Scale = shipTransform.Scale // Keep the same scale
                    };
                    
                    TransformLookup[entity] = postTransform;
        
                    receiver.currentKnockbackVelocity *= math.exp(-5f * DeltaTime);

                    if (math.lengthsq(receiver.currentKnockbackVelocity) < 0.01f || receiver.currentRecoveryTime <= 0)
                    {
                        receiver.isBeingKnockedBack = false;

                        receiver.currentKnockbackVelocity = float3.zero;
                    }

                }


                receiver.currentRecoveryTime -= DeltaTime;
                return;
            }
            

            if (math.distance(shipTransform.Position, TargetLocation) < followTarget.FollowTargetLimits)
            {
                return;
            }

            // Calculate repulsion from nearby cells
            var repulsionForce = float3.zero;
            var repulsionRadius = 2.5f; // Adjust based on your entity size

            // Get the current cell and neighboring cells
            var currentCell = gridData.GridCell;

            for (var i = 0; i < neighborOffsets.Length; i++)
            {
                var neighborCell = currentCell + neighborOffsets[i];

                // Check entities in this neighbor cell
                if (!CellToEntities.TryGetFirstValue(neighborCell, out Entity neighborEntity, out var iterator))
                    continue;
                do
                {
                    // Skip self
                    if (neighborEntity.Equals(entity)) continue;

                    // Get position and calculate distance
                    var otherPosition = TransformLookup[neighborEntity].Position;
                    var direction = shipTransform.Position - otherPosition;
                    var distance = math.length(direction);

                    // Apply repulsion if within radius
                    if (!(distance > 0) || !(distance < repulsionRadius)) continue;
                    var repulsionStrength = (repulsionRadius - distance) / repulsionRadius;
                    repulsionForce += math.normalize(direction) * repulsionStrength;
                } while (CellToEntities.TryGetNextValue(out neighborEntity, ref iterator));
            }

            // Combine target direction with repulsion
            float3 targetDirection = math.normalize(TargetLocation - shipTransform.Position);
            float3 finalDirection = math.normalize(targetDirection + repulsionForce * 1.5f); // Adjust weight as needed


            var calcExp = DeltaTime * followTarget.Speed;
            var calculatedPosition = math.lerp(
                shipTransform.Position,
                shipTransform.Position + finalDirection * calcExp,
                calcExp);
            
            LocalTransform newTransform = new LocalTransform
            {
                Position = new float3(calculatedPosition.x, 0, calculatedPosition.z),
                Rotation = math.slerp(
                    shipTransform.Rotation,
                    quaternion.LookRotation(finalDirection, math.up()),
                    DeltaTime * followTarget.RotationSpeed),
                Scale = shipTransform.Scale // Keep the same scale
            };

            TransformLookup[entity] = newTransform;
            
        }
    }
}
