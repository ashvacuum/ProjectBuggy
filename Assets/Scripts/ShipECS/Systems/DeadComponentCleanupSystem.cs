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
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    internal partial struct DeadComponentCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            //state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DeadComponentTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var vfxExplosionSingleton = SystemAPI.GetSingletonRW<VFXExplosionsSingleton>().ValueRW;
            var vfxThrustersSingleton = SystemAPI.GetSingletonRW<VFXThrustersSingleton>().ValueRW;
            
            var rocketDeathJob = new RocketDeathJob
            {
                ThrustersManager = vfxThrustersSingleton.Manager
            };
            state.Dependency = rocketDeathJob.Schedule(state.Dependency);
            
            var normalShipDeathJob = new NormalShipDeathJob
            {
                ExplosionsManager = vfxExplosionSingleton.Manager
            };
            state.Dependency = normalShipDeathJob.Schedule(state.Dependency);
            
            var finalizeDeathJob = new FinalizedDeathJob
            {
                ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            };
            state.Dependency = finalizeDeathJob.ScheduleParallel(state.Dependency);
            /*
            var finalizeDeathHealthJob = new FinalizedDeathJobViaHealth()
            {
                ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged),
                ExplosionsManager = vfxExplosionSingleton.Manager,
                ThrustersManager = vfxThrustersSingleton.Manager
            };
            state.Dependency = finalizeDeathHealthJob.Schedule(state.Dependency);*/
            //state.Dependency.Complete();
        }
        
        
        [BurstCompile]
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
        public partial struct RocketDeathJob : IJobEntity
        {
            public VFXManagerParented<VFXRocketData> ThrustersManager;
            
            public void Execute(Entity entity, in LocalTransform transform, in DeadComponentTag tag, in RocketTrailData rocket)
            {
                ThrustersManager.Kill(rocket.RocketVFXIndex);
            }
        }
        
        [BurstCompile]
        [WithNone(typeof(ProjectileMotion))]
        public partial struct FinalizedDeathJobViaHealth : IJobEntity
        {
            public EntityCommandBuffer ECB;
            public VFXManager<VFXExplosionRequest> ExplosionsManager;
            public VFXManagerParented<VFXRocketData> ThrustersManager;

            private int _chunkIndex;

            public void Execute(Entity entity, in HealthComponent health, in LocalTransform transform, in RocketTrailData rocket)
            {
                if (health.CurrentHealth < 0)
                {
                    
                    Random random = Random.CreateFromIndex((uint)entity.Index);
                    float explosionSize =
                        random.NextFloat(100, 150f);
                    //Debug.Log($"Destroying Entity: {entity.Index}");
                    ECB.DestroyEntity(entity);
                    ExplosionsManager.AddRequest(new VFXExplosionRequest
                    {
                        Position = transform.Position,
                        Scale = explosionSize,
                    });
                    
                    ThrustersManager.Kill(rocket.RocketVFXIndex);
                }
            }

            
        }

        [BurstCompile]
        public partial struct FinalizedDeathJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private int _chunkIndex;

            public void Execute(Entity entity, in DeadComponentTag dead)
            {
                Debug.Log($"Destroying Entity: {entity.Index}");
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
