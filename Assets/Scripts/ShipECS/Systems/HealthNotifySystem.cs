using Authoring;
using NonECS.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    
    public struct HealthChangedTag : IComponentData {}
    
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            var vfxSparksSingleton = SystemAPI.GetSingletonRW<VFXHitSparksSingleton>().ValueRW;
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<DamageNumberRequest>(out var dmgBuffer, false);
            if (!hasBuffer) return;
            var nonPlayerHealthJob = new NonPlayerHealthJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ECB = ecb,
                HitSparksManager = vfxSparksSingleton.Manager,
                DamageBuffers = dmgBuffer,
                PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true)
            };
                
            state.Dependency = nonPlayerHealthJob.Schedule(state.Dependency);
            state.Dependency.Complete();
            //nonPlayerHealthJob.C
            /*

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
            */
        }
    }

    public struct DeadComponentTag : ICleanupComponentData { }

    [BurstCompile]
    [WithNone(typeof(DeadComponentTag))]
    public partial struct NonPlayerHealthJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ECB;
        public VFXManager<VFXHitSparksRequest> HitSparksManager;
        public DynamicBuffer<DamageNumberRequest> DamageBuffers;
        [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
        
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
            }
            
            health.WasDamagedCritical = false;
            health.PreviousHealth = health.CurrentHealth;

            if (health.CurrentHealth <= 0)
            {
                ECB.AddComponent<DeadComponentTag>(entity);
            } else if(!PlayerLookup.HasComponent(entity))
            {
                HitSparksManager.AddRequest(new VFXHitSparksRequest
                {
                    Position = transform.Position,
                    Color = new float3(255, 255, 0),
                });
            }
        }
    }
}
