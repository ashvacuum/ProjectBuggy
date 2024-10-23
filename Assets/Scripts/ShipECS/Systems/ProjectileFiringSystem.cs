using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ShipECS.Systems
{
    
    public partial struct ProjectileFiringSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyTrackingComponent>();
        }
        
        public void OnUpdate(ref SystemState state)
        {

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var enemyTrackingComponent = SystemAPI.GetSingleton<EnemyTrackingComponent>();

            foreach (var (transform, weapon) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ProjectileAttack>>()
                         .WithAll<ShipComponent>())
            {

                if (weapon.ValueRO.CurrentFireRate > 0)
                {
                    weapon.ValueRW.CurrentFireRate -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                weapon.ValueRW.CurrentFireRate = weapon.ValueRO.TotalFireRate;

                for (var i = 0; i < weapon.ValueRO.TotalCount; i++)
                {
                    var instance = ecb.Instantiate(weapon.ValueRO.ProjectilePrefab);
                    ecb.AddComponent(instance, new DamageComponent()
                    {
                        Damage = weapon.ValueRO.TotalDamage
                    });

                    var direction = math.normalize(enemyTrackingComponent.TrackingTargetPosition -
                                                   transform.ValueRO.Position);
                    ecb.AddComponent(instance, new ProjectileMotion()
                    {
                        Direction = direction,
                        Speed = weapon.ValueRO.TotalSpeed,
                        LifeTime = weapon.ValueRO.TotalLifeTime
                    });
                    ecb.AddComponent(instance, new HealthComponent()
                    {
                        MaxHealth = weapon.ValueRO.TotalPenetration,
                        CurrentHealth = weapon.ValueRO.TotalPenetration,
                        CurrentNextTimeToTakeDamage = 0,
                        NextTimeToTakeDamage = 0,
                        PreviousHealth = weapon.ValueRO.TotalPenetration
                    });
                    ecb.SetComponent(instance, new LocalTransform()
                    {
                        Position = transform.ValueRO.Position,
                        Rotation = quaternion.LookRotation(direction, math.up()),
                        Scale = 1
                    });

                }

            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
    
    public struct ProjectileAttack  : IComponentData, IEnableableComponent
    {
        public Entity ProjectilePrefab;
        
        public float BaseFireRate;
        public float FireRateReductionBonus;
        public float TotalFireRate => BaseFireRate - FireRateReductionBonus/100f * BaseFireRate;
        
        public int BaseNumProjectile;
        public int NumProjectileBonus; 
        public int TotalCount => BaseNumProjectile + NumProjectileBonus;
        
        public int BasePenetration;
        public int PenetrationBonus; 
        public int TotalPenetration => math.max(1,PenetrationBonus + BasePenetration);
        
        public float BaseDamage;
        public float DamageBonus; 
        public float TotalDamage => BaseDamage + (DamageBonus/100f * BaseDamage);
        
        public float BaseSize;
        public float SizeBonus;
        public float TotalSize =>  BaseSize + (SizeBonus/100f * BaseSize);
        
        public float BaseLifeTime;
        public float UnitLifeTimeBonus;
        public float TotalLifeTime => BaseLifeTime + (UnitLifeTimeBonus/100 * BaseLifeTime);
        
        public float BaseSpeed;
        public float SpeedBonus; 
        public float TotalSpeed => BaseSpeed + SpeedBonus/100f * BaseSpeed;


        public float CurrentFireRate; // value to edit if it hits 0 it will fire and reset to total fire rate
    }
    

    //involves stats that the player itself has that increments the bonuses on the weapons that have a stat that aligns with it, involves the upgrade system
    public struct PlayerBonusStat : IComponentData
    {
        public float LifetimeBonus;
        public int NumCountBonus; 
        public int PenetrationBonus;
        public float DamageBonus;
        public float SpeedBonus;
        public float AttackTimeReductionBonus;
        public float SizeBonus;
    }

    public struct WeaponLifetimeStat : IComponentData
    {
        public float BaseUnitLifeTime;
        public float UnitLifeTimeBonus; 
        public float TotalLifeTime => BaseUnitLifeTime + (UnitLifeTimeBonus/100 * BaseUnitLifeTime);
    }
    
    public struct WeaponCountStat : IComponentData
    {
        public int BaseNumCount;
        public int NumCountBonus; 
        public int TotalCount => BaseNumCount + NumCountBonus;
    }
    
    public struct WeaponPenetrationStat : IComponentData
    {
        public int BaseNumCount;
        public int NumCountBonus; 
        public int TotalCount => math.max(1,BaseNumCount + NumCountBonus);
    }
    
    public struct WeaponDamageStat : IComponentData
    {
        public float BaseDamage;
        public float DamageBonus; 
        public float TotalDamage => BaseDamage + (DamageBonus/100f * BaseDamage);
    }

    public struct WeaponSpeedStat : IComponentData
    {
        public float BaseSpeed;
        public float SpeedBonus; 
        public float TotalSpeed => BaseSpeed + SpeedBonus/100f * BaseSpeed;
    }
    
    public struct WeaponFireRateStat : IComponentData
    {
        public float BaseInterval;
        public float IntervalReductionBonus;
        public float TotalFireRate => BaseInterval - IntervalReductionBonus/100f * BaseInterval;
    }
    
    public struct WeaponSizeStat : IComponentData
    {
        public float BaseSize;
        public float SizeBonus;
        public float TotalSize =>  BaseSize + (SizeBonus/100f * BaseSize);
    }
}
