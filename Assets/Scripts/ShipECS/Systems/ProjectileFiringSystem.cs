using Authoring;
using Authoring.Projectiles;
using NonECS.BaseWeapons;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct ProjectileFiringSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ProjectileSpawnerComponent>();
        }
        
        public void OnUpdate(ref SystemState state)
        {

            var ecb =  SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var projectileSpawnerData = SystemAPI.GetSingletonBuffer<ProjectileSpawnerComponent>();
            
            
            var enemyTargetBuffers = SystemAPI.GetSingletonBuffer<EnemyTargetPoints>();

            foreach (var contents in SystemAPI.Query<RefRO<ProjectileMotion>, RefRO<NewSpawnRenderInvisibleTag>>().WithEntityAccess()
                         .WithNone<DeadComponentTag>())
            {
                //Debug.Log("Fixing Flicker issue");
                RenderingFlickerFixUtil.BeginRecursiveLayerChange<NewSpawnRenderInvisibleTag>(ref state, contents.Item3, ecb);
            }
            
            if (!ProjectileHelper.TryGetEntityFromWeaponClass(projectileSpawnerData, WeaponClass.Projectile, out var entityToSpawn))
            {
                return;
            }
            
            foreach (var (projectileRef, bonus, transform) in
                     SystemAPI.Query<RefRW<ProjectileAttack>, RefRO<PlayerBonusStat>, RefRW<LocalTransform>>())
            {
                var projectile = new ProjectileFiringAspect(projectileRef, bonus, transform);
                if (projectile.CurrentFireRate > 0)
                {
                    projectile.CurrentFireRate -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                projectile.CurrentFireRate = projectile.TotalFireRate;
                var totalCountWeapons = math.min(enemyTargetBuffers.Length, projectile.TotalCount);
                for (var i = 0; i < totalCountWeapons; i++)
                {
                    if (enemyTargetBuffers[i].Distance > projectile.TotalRange) continue;
                    var instance = ecb.Instantiate(entityToSpawn);
                    ecb.AddComponent(instance, new DamageComponent()
                    {
                        Damage = projectile.TotalDamage,
                        CriticalChance = projectile.TotalCritical
                    });
                    
                    var targetPos = enemyTargetBuffers[i].Position;
                    
                    var direction = math.normalize(targetPos - projectile.Position);
                    ecb.AddComponent(instance, new ProjectileMotion()
                    {
                        Direction = direction,
                        Speed = projectile.TotalSpeed,
                        LifeTime = projectile.TotalLifeTime
                    });
                    ecb.AddComponent(instance, new HealthComponent()
                    {
                        MaxHealth = projectile.TotalPenetration,
                        CurrentHealth = projectile.TotalPenetration,
                        CurrentNextTimeToTakeDamage = 0,
                        NextTimeToTakeDamage = 0,
                        PreviousHealth = projectile.TotalPenetration
                    });
                    ecb.SetComponent(instance, new LocalTransform()
                    {
                        Position = projectile.Position,
                        Rotation = quaternion.LookRotation(direction, math.up()),
                        Scale = projectile.TotalSize
                    });
                    ecb.AddComponent(instance, new KnockbackSender()
                    {
                        knockbackForceToSend = projectile.TotalKnockback
                    });
                    
                    //RenderingFlickerFixUtil.BeginRecursiveLayerChange<NewSpawnRenderInvisibleTag>(ref state, instance, ecb);

                }

            }
            enemyTargetBuffers.Clear();
            //ecb.Playback(state.EntityManager);
            //ecb.Dispose();
        }

        
        
        
        
    }

    public readonly struct ProjectileFiringAspect
    {
        private readonly RefRW<ProjectileAttack> _projectile;
        private readonly RefRO<PlayerBonusStat> _bonusStats;
        private readonly RefRW<LocalTransform> _transform;

        public ProjectileFiringAspect(RefRW<ProjectileAttack> projectile, RefRO<PlayerBonusStat> bonusStats,
            RefRW<LocalTransform> transform)
        {
            _projectile = projectile;
            _bonusStats = bonusStats;
            _transform = transform;
        }

        public float TotalFireRate => math.max(0,
            _projectile.ValueRW.BaseFireRate -
            _bonusStats.ValueRO.FireRateReductionBonus / 100f * _projectile.ValueRW.BaseFireRate);
        
        public int TotalCount => _projectile.ValueRO.BaseNumProjectile + _bonusStats.ValueRO.NumCountBonus;
        public int TotalPenetration => math.max(1,_projectile.ValueRO.BasePenetration + _bonusStats.ValueRO.PenetrationBonus);
        public float TotalDamage => _projectile.ValueRO.BaseDamage + (_bonusStats.ValueRO.DamageBonus/100f * _projectile.ValueRO.BaseDamage);
        public float TotalRange => _projectile.ValueRO.BaseRange + (_bonusStats.ValueRO.RangeBonus/100f * _projectile.ValueRO.BaseRange);
        public float TotalSize =>  _projectile.ValueRO.BaseSize + (_bonusStats.ValueRO.SizeBonus/100f * _projectile.ValueRO.BaseSize);
        public float TotalLifeTime => _projectile.ValueRO.BaseLifeTime + (_bonusStats.ValueRO.LifetimeBonus/100 * _projectile.ValueRO.BaseLifeTime);
        public float TotalSpeed => _projectile.ValueRO.BaseSpeed + _bonusStats.ValueRO.SpeedBonus/100f * _projectile.ValueRO.BaseSpeed;
        public float TotalKnockback => _projectile.ValueRO.BaseKnockback - _bonusStats.ValueRO.KnockbackBonus/100f * _projectile.ValueRO.BaseKnockback;
        public float3 Position => _transform.ValueRO.Position;
        public float TotalCritical => _projectile.ValueRO.BaseCritical + (_bonusStats.ValueRO.CriticalBonus / 100f * _projectile.ValueRO.BaseCritical);
        
        public float CurrentFireRate
        {
            get => _projectile.ValueRO.CurrentFireRate;
            set => _projectile.ValueRW.CurrentFireRate = value;
        }

    }
    
    public struct ProjectileAttack  : IComponentData
    {
        public float BaseFireRate;
        public int BaseNumProjectile;
        public int BasePenetration;
        public float BaseDamage;
        public float BaseRange;
        public float BaseSize;
        public float BaseLifeTime;
        public float BaseSpeed;
        public float BaseKnockback;
        public float BaseCritical;
        public float CurrentFireRate; // value to edit if it hits 0 it will fire and reset to total fire rate
    }

     static class ProjectileHelper
    {
        public static bool TryGetEntityFromWeaponClass(DynamicBuffer<ProjectileSpawnerComponent> spawner, WeaponClass weapon, out Entity projectileToSpawn)
        {
            foreach (var data in spawner)
            {
                if (data.Class != weapon) continue;
                projectileToSpawn = data.ProjectileToSpawn;
                return true;
            }
            
            projectileToSpawn = Entity.Null;
            return false;
        }
    }
}
