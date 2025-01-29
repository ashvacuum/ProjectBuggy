using Authoring;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ShipECS.Systems
{
    //[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    internal partial struct DeadComponentCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DeadComponentTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            VFXExplosionsSingleton vfxExplosionSingleton = SystemAPI.GetSingletonRW<VFXExplosionsSingleton>().ValueRW;
            VFXThrustersSingleton vfxThrustersSingleton = SystemAPI.GetSingletonRW<VFXThrustersSingleton>().ValueRW;
            
            ShipRocketDeathJob shipRocketDeathJob = new ShipRocketDeathJob
            {
                ExplosionsManager = vfxExplosionSingleton.Manager,
                ThrustersManager = vfxThrustersSingleton.Manager
            };
            state.Dependency = shipRocketDeathJob.Schedule(state.Dependency);
            
            NormalShipDeathJob normalShipDeathJob = new NormalShipDeathJob
            {
                ExplosionsManager = vfxExplosionSingleton.Manager
            };
            state.Dependency = normalShipDeathJob.Schedule(state.Dependency);
            
            FinalizedDeathJob finalizeDeathJob = new FinalizedDeathJob
            {
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            };
            state.Dependency = finalizeDeathJob.ScheduleParallel(state.Dependency);

            
        }
        
        
        [BurstCompile]
        [WithNone(typeof(RocketTrailData))]
        public partial struct NormalShipDeathJob : IJobEntity
        {
            public VFXManager<VFXExplosionRequest> ExplosionsManager;
            
            public void Execute(Entity entity, in LocalTransform transform, in DeadComponentTag tag,
                in EnemyFollowTarget enemyTag)
            {
                // Explosion
                Random random = Random.CreateFromIndex((uint)entity.Index);
                float explosionSize =
                    random.NextFloat(100, 150f);
                ExplosionsManager.AddRequest(new VFXExplosionRequest
                {
                    Position = transform.Position,
                    Scale = explosionSize,
                });
            }
        }
        
        [BurstCompile]
        public partial struct ShipRocketDeathJob : IJobEntity
        {
            public VFXManager<VFXExplosionRequest> ExplosionsManager;
            public VFXManagerParented<VFXRocketData> ThrustersManager;
            
            public void Execute(Entity entity, in LocalTransform transform, in DeadComponentTag tag,
                in EnemyFollowTarget enemyTag, in RocketTrailData rocket)
            {
                // Explosion
                Random random = Random.CreateFromIndex((uint)entity.Index);
                float explosionSize =
                    random.NextFloat(100, 150f);
                ExplosionsManager.AddRequest(new VFXExplosionRequest
                {
                    Position = transform.Position,
                    Scale = explosionSize,
                });
                
                ThrustersManager.Kill(rocket.RocketVFXIndex);
            }
        }

        [BurstCompile]
        public partial struct FinalizedDeathJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private int _chunkIndex;

            public void Execute(Entity entity, in DeadComponentTag dead)
            {
                ECB.DestroyEntity(_chunkIndex, entity);
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask,
                bool chunkWasExecuted)
            {
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                _chunkIndex = unfilteredChunkIndex;
                return true;
            }
        }

        
    }
}
