using Authoring;
using Unity.Collections;
using Unity.Entities;
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
            state.RequireForUpdate<ProjectileSpawnerComponent>();
        }
        
        public void OnUpdate(ref SystemState state)
        {

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var projectileSpawnerData = SystemAPI.GetSingleton<ProjectileSpawnerComponent>();

            var enemyTargetBuffers = SystemAPI.GetSingletonBuffer<EnemyTargetPoints>();
            
            foreach (var projectile in SystemAPI.Query<ProjectileFiringAspect>())
            {
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
                    var instance = ecb.Instantiate(projectileSpawnerData.ProjectileToSpawn);
                    ecb.AddComponent(instance, new DamageComponent()
                    {
                        Damage = projectile.TotalDamage
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

                }

            }
            enemyTargetBuffers.Clear();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public readonly partial struct ProjectileFiringAspect : IAspect
    {
        private readonly RefRW<ProjectileAttack> _projectile;
        private readonly RefRO<PlayerBonusStat> _bonusStats;
        private readonly RefRW<LocalTransform> _transform;

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
        
        public float CurrentFireRate
        {
            get => _projectile.ValueRO.CurrentFireRate;
            set => _projectile.ValueRW.CurrentFireRate = value;
        }

    }
    
    public struct ProjectileAttack  : IComponentData, IEnableableComponent
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
        public float CurrentFireRate; // value to edit if it hits 0 it will fire and reset to total fire rate
    }
}
