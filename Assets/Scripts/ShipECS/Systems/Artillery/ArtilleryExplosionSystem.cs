using Authoring;
using ShipECS.Systems.Damage;
using Unity.Collections;
using Unity.Entities;
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
            
            foreach (var artilleryFiringAspect in SystemAPI.Query<ArtilleryFiringAspect>())
            {
                foreach (var (explosionTag, transform, entity) in
                         SystemAPI.Query<RefRO<ArtilleryExplosionTag>, RefRO<LocalTransform>>().WithEntityAccess().WithNone<DeadComponentTag>())
                {
                    var healthLookup = _health;
                    var hitResults = new NativeList<ColliderCastHit>(Allocator.Temp);
                    var collisionFilter = new CollisionFilter()
                    {
                        BelongsTo = 1 << 1, CollidesWith = 1 << 2 & 1 << 3, GroupIndex = 0
                    };
                    var hasHit = collisionWorld.SphereCastAll(transform.ValueRO.Position,
                        artilleryFiringAspect.TotalRange,
                        transform.ValueRO.Forward(),
                        0,
                        ref hitResults,
                        collisionFilter);

                    if (!hasHit)
                    {
                        Debug.Log("No Hits Detected");
                        continue;
                    }
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
                            healthLookup[entity] = health;
                        }

                        if (_knockBackReceiver.HasComponent(hit.Entity))
                        {
                            //TODO: implement knockback system
                        }
                    }

                    hitResults.Dispose();
                    ecb.AddComponent<DeadComponentTag>(entity);

                }

                break;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
