using Authoring;
using ShipECS.Systems.Damage;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ShipECS.Systems.Artillery
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateAfter(typeof(ArtilleryMovementSystem))]
    public partial struct ArtilleryExplosionSystem : ISystem
    {
        private ComponentLookup<HealthComponent> _health;
        private ComponentLookup<KnockBackReceiver> _knockBackReceiver;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            //stores up to 1000 hits
            _health = state.GetComponentLookup<HealthComponent>();
            _knockBackReceiver = state.GetComponentLookup<KnockBackReceiver>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            _health.Update(ref state);
            _knockBackReceiver.Update(ref state);
            // Create a CollisionWorld reference
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var collisionWorld = physicsWorldSingleton.CollisionWorld;
            var random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime * 1000);
            var hitResults = new NativeList<ColliderCastHit>(Allocator.Temp);
            foreach (var artilleryFiringAspect in SystemAPI.Query<ArtilleryFiringAspect>())
            {
                foreach (var (explosionTag, transform, entity) in
                         SystemAPI.Query<RefRO<ArtilleryExplosionTag>, RefRO<LocalTransform>>().WithEntityAccess()
                             .WithNone<DeadComponentTag>())
                {
                    ecb.AddComponent<DeadComponentTag>(entity);
                    var healthLookup = _health;

                    var collisionFilter = new CollisionFilter()
                    {
                        BelongsTo = 1u << 0, CollidesWith = (1u << 2) | (1u << 3), GroupIndex = 0
                    };
                    var hasHit = collisionWorld.SphereCastAll(transform.ValueRO.Position,
                        artilleryFiringAspect.TotalRange,
                        transform.ValueRO.Forward(),
                        0,
                        ref hitResults,
                        collisionFilter);

                    if (!hasHit)
                    {
                        Debug.Log($"No Hits Detected Range {artilleryFiringAspect.TotalRange}");
                        continue;
                    }

                    var tempJob = new NativeList<ColliderCastHit>(Allocator.TempJob);
                    tempJob.AddRange(hitResults);
                    
                    var job = new ProcessHitsJob
                    {
                        Hits = tempJob,
                        SourceEntity = entity,
                        Transform = SystemAPI.GetComponentLookup<LocalTransform>(true),
                        MassOverride = SystemAPI.GetComponentLookup<PhysicsMass>(true),
                        KnockBackReceiver = SystemAPI.GetComponentLookup<KnockBackReceiver>(),
                        HealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(),
                        Random = random,
                        KnockbackForce = artilleryFiringAspect.TotalKnockback,
                        TotalDamage = artilleryFiringAspect.TotalDamage,
                        TotalCritical = artilleryFiringAspect.TotalCritical
                    };


                    var handle = job.Schedule(state.Dependency);

                    state.Dependency = handle;

                    /*
                    Debug.Log($"Found Hits {hitResults.Length}");
                    foreach (var hit in hitResults)
                    {
                        if (healthLookup.HasComponent(hit.Entity))
                        {
                            var rolledChance = random.NextFloat(0, 100);
                            var health = healthLookup[hit.Entity];
                            if (health.CurrentHealth <= 0 || !(health.CurrentNextTimeToTakeDamage <= 0)) return;
                            if (artilleryFiringAspect.TotalCritical > 0 &&
                                rolledChance < artilleryFiringAspect.TotalCritical)
                            {
                                Debug.Log($"Rolled {rolledChance}/{artilleryFiringAspect.TotalCritical} ");
                                health.CurrentHealth -=
                                    artilleryFiringAspect.TotalDamage * DamageConstants.CritMultiplier;
                                health.WasDamagedCritical = true;
                            }
                            else
                            {
                                health.CurrentHealth -= artilleryFiringAspect.TotalDamage;
                                health.WasDamagedCritical = false;
                            }

                            health.CurrentNextTimeToTakeDamage = health.NextTimeToTakeDamage;
                            healthLookup[hit.Entity] = health;
                        }

                        if (_knockBackReceiver.HasComponent(hit.Entity))
                        {
                            //TODO: implement knockback system
                        }
                    }*/

                    //hitResults.Dispose();


                }

                break;
            }

            hitResults.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public struct ProcessHitsJob : IJob
    {
        [ReadOnly] public NativeList<ColliderCastHit> Hits;
        [ReadOnly] public Entity SourceEntity;
        [ReadOnly] public ComponentLookup<LocalTransform> Transform;
        [ReadOnly] public ComponentLookup<PhysicsMass> MassOverride;
        public ComponentLookup<KnockBackReceiver> KnockBackReceiver;
        public ComponentLookup<HealthComponent> HealthLookup;
        public float KnockbackForce;
        public float TotalDamage;
        public float TotalCritical;
        public Unity.Mathematics.Random Random;

        public void Execute()
        {
            for (int i = 0; i < Hits.Length; i++)
            {
                var hit = Hits[i];
                var hitEntity = hit.Entity;

                // Skip if hit entity is invalid or is the source
                if (hitEntity == Entity.Null || hitEntity == SourceEntity) continue;



                // Process Health/Damage
                if (HealthLookup.HasComponent(hitEntity))
                {
                    var health = HealthLookup[hitEntity];

                    // Skip if already dead or on damage cooldown
                    if (health.CurrentHealth <= 0 || health.CurrentNextTimeToTakeDamage > 0)
                        continue;

                    // Roll for critical hit
                    var rolledChance = Random.NextFloat(0, 100);
                    if (TotalCritical > 0 && rolledChance < TotalCritical)
                    {
                        health.CurrentHealth -= TotalDamage * DamageConstants.CritMultiplier;
                        health.WasDamagedCritical = true;
                    }
                    else
                    {
                        health.CurrentHealth -= TotalDamage;
                        health.WasDamagedCritical = false;
                    }

                    health.CurrentNextTimeToTakeDamage = health.NextTimeToTakeDamage;
                    HealthLookup[hitEntity] = health;
                }

                // Early exit if we don't have required components for knockback
                if (!Transform.HasComponent(hitEntity)) continue;

                // Process Knockback
                if (KnockBackReceiver.HasComponent(hitEntity))
                {
                    var knockBack = KnockBackReceiver[hitEntity];

                    // Skip if still in recovery
                    if (knockBack.currentRecoveryTime > 0) continue;

                    var isKinematic = MassOverride.HasComponent(hitEntity) && MassOverride[hitEntity].IsKinematic;

                    // Calculate knockback direction using the hit normal
                    var knockBackDirection = math.normalize(hit.SurfaceNormal);
                    var knockBackVelocity = knockBackDirection * KnockbackForce;

                    if (isKinematic)
                    {
                        knockBack.currentKnockbackVelocity = knockBackVelocity;
                        knockBack.isBeingKnockedBack = true;
                    }

                    knockBack.currentRecoveryTime = knockBack.recoveryTime;
                    KnockBackReceiver[hitEntity] = knockBack;
                }
            }
        }
    }

    

}

