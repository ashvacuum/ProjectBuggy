using Authoring;
using NonECS.UI;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    
    public struct HealthChangedTag : IComponentData {}
    
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {/*
            var vfxSparksSingleton = SystemAPI.GetSingletonRW<VFXHitSparksSingleton>().ValueRW;
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<DamageNumberRequest>(out var dmgBuffer, false);
            if (!hasBuffer) return;
            
            var nonPlayerHealthJob = new NonPlayerHealthJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB =  new EntityCommandBuffer(Allocator.TempJob).AsParallelWriter(),
                HitSparksManager = vfxSparksSingleton.Manager,
                DamageBuffers = dmgBuffer,
                PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true)
            };
                
            state.Dependency = nonPlayerHealthJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();*/
            
            //nonPlayerHealthJob.C

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<DamageNumberRequest>(out var dmgBuffer, false);
            foreach (var (health, transform,entity) in SystemAPI.Query<RefRW<HealthComponent>, RefRO<LocalTransform>>().WithEntityAccess().WithNone<DeadComponentTag>())
            {
                if (health.ValueRO.CurrentNextTimeToTakeDamage > 0)
                {
                    health.ValueRW.CurrentNextTimeToTakeDamage -= SystemAPI.Time.DeltaTime;
                }

                var difference = math.abs(health.ValueRO.CurrentHealth - health.ValueRO.PreviousHealth);
                if (difference < 0.1) continue;
                
                //we shouldnt be adding damage numbers to our player
                if (state.EntityManager.HasComponent<PlayerTag>(entity))
                {
                } else
                {
                    if (hasBuffer)
                    {
                        dmgBuffer.Add(new DamageNumberRequest()
                        {
                            DamageAmount = difference,
                            WorldPosition = transform.ValueRO.Position,
                            IsCritical = health.ValueRO.WasDamagedCritical
                            
                        });
                    }
                }

                health.ValueRW.WasDamagedCritical = false;
                health.ValueRW.PreviousHealth = health.ValueRO.CurrentHealth;

                
                
                if (health.ValueRO.CurrentHealth <= 0)
                {
                    ecb.AddComponent<DeadComponentTag>(entity);
                }
                
                
                //Debug.Log($"Created Health Change Tag: {health.ValueRO.CurrentHealth} {health.ValueRO.PreviousHealth} : {difference}");
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
        }
    }

    public struct DeadComponentTag : ICleanupComponentData { }

    [BurstCompile]
    [WithNone(typeof(DeadComponentTag),typeof(ProjectileMotion))]
    public partial struct NonPlayerHealthJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        public VFXManager<VFXHitSparksRequest> HitSparksManager;
        public DynamicBuffer<DamageNumberRequest> DamageBuffers;
        [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
        private int _chunkIndex;
        public void Execute(Entity entity, ref HealthComponent health, ref LocalTransform transform)
        {
            if (health.CurrentNextTimeToTakeDamage > 0)
            {
                health.CurrentNextTimeToTakeDamage -= DeltaTime;
                return;
            }
            
            var difference = math.abs(health.CurrentHealth - health.PreviousHealth);
            if (difference < 0.1) return;

            if (!PlayerLookup.HasComponent(entity))
            {
                DamageBuffers.Add(new DamageNumberRequest()
                {
                    DamageAmount = difference,
                    WorldPosition = transform.Position,
                    IsCritical = health.WasDamagedCritical
                });
                
                //Debug.Log("Added damage Buffers");
            }
            
            health.WasDamagedCritical = false;
            health.PreviousHealth = health.CurrentHealth;

            if (health.CurrentHealth <= 0)
            {
                ECB.AddComponent<DeadComponentTag>(_chunkIndex,entity);
                Debug.Log($"Added dead component: {entity.Index}");
            } else if(!PlayerLookup.HasComponent(entity))
            {
                HitSparksManager.AddRequest(new VFXHitSparksRequest
                {
                    Position = transform.Position,
                    Color = new float3(255, 255, 0),
                });
            }
            
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
    /*
    [BurstCompile]
    public partial struct HealthUpdateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float DeltaTime;
        public Entity DamageBufferEntity; // Singleton entity holding the DamageNumberRequest buffer

        public void Execute(
            [EntityIndexInQuery] int sortKey,
            Entity entity,
            ref HealthComponent health,
            in LocalTransform transform,
            [Optional] in PlayerTag playerTag // Optional tag (check if it exists)
        )
        {
            // Update health timer
            if (health.CurrentNextTimeToTakeDamage > 0)
                health.CurrentNextTimeToTakeDamage -= DeltaTime;

            // Skip if health difference is negligible
            float difference = math.abs(health.CurrentHealth - health.PreviousHealth);
            if (difference < 0.1f) return;

            // Add damage number to buffer if NOT a player
            if (!playerTag.Equals(default)) // Check if PlayerTag exists
            {
                ECB.AppendToBuffer(sortKey, DamageBufferEntity, new DamageNumberRequest
                {
                    DamageAmount = difference,
                    WorldPosition = transform.Position,
                    IsCritical = health.WasDamagedCritical
                });
            }

            // Update health state
            health.WasDamagedCritical = false;
            health.PreviousHealth = health.CurrentHealth;

            // Add DeadComponentTag if health <= 0
            if (health.CurrentHealth <= 0)
                ECB.AddComponent<DeadComponentTag>(sortKey, entity);
        }
    }
    */
}
