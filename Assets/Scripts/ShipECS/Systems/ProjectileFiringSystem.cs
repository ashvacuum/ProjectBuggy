using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

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

            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<ShipComponent>())
            {
                foreach (var weapon in
                         SystemAPI.Query<ProjectileWeaponAspect>())
                {
                    if (weapon.FireRate.ValueRO.CurrentRate > 0)
                    {
                        weapon.FireRate.ValueRW.CurrentRate -= SystemAPI.Time.DeltaTime;
                        continue;
                    }

                    weapon.FireRate.ValueRW.CurrentRate = weapon.FireRate.ValueRO.TotalFireRate;

                    for (var i = 0; i < weapon.NumProjectiles.ValueRO.TotalCount; i++)
                    {
                        var instance = ecb.Instantiate(weapon.Projectile);
                        ecb.AddComponent(instance, new DamageComponent()
                        {
                            Damage = weapon.Damage.ValueRO.TotalDamage
                        });

                        var direction = math.normalize(enemyTrackingComponent.TrackingTargetPosition -
                                                       weapon.LocalTransform.ValueRO.Position);
                        ecb.AddComponent(instance, new ProjectileMotion()
                        {
                            Direction = direction,
                            Speed = weapon.Speed.ValueRO.TotalSpeed,
                            LifeTime = weapon.Lifetime.ValueRO.TotalLifeTime
                        });
                        ecb.AddComponent(instance, new HealthComponent()
                        {
                            MaxHealth = weapon.Penetration.ValueRO.TotalCount,
                            CurrentHealth = weapon.Penetration.ValueRO.TotalCount
                        });
                        ecb.SetComponent(instance, new LocalTransform()
                        {
                            Position = transform.ValueRO.Position,
                            Rotation = quaternion.LookRotation(direction, math.up()),
                            Scale = 1
                        });
                    }
                }

            }
        }

        
    }
    
    
    public struct ProjectileAttack  : IComponentData, IEnableableComponent
    {
        public Entity ProjectilePrefab;
    }
    public struct LazerAttack  : IComponentData, IEnableableComponent
    {
        public Entity ProjectilePrefab;
    }
    public struct ArtilleryAttack : IComponentData, IEnableableComponent
    {
        public Entity ProjectilePrefab;
    }

    public struct WeaponLifetime : IComponentData
    {
        public float BaseUnitLifeTime;
        public float UnitLifeTimeBonus; 
        public float TotalLifeTime => BaseUnitLifeTime + (UnitLifeTimeBonus/100 * UnitLifeTimeBonus);
    }
    
    public struct WeaponCount : IComponentData
    {
        public int BaseNumCount;
        public int NumCountBonus; 
        public int TotalCount => BaseNumCount + NumCountBonus;
    }
    
    public struct WeaponPenetration : IComponentData
    {
        public int BaseNumCount;
        public int NumCountBonus; 
        public int TotalCount => math.max(1,BaseNumCount + NumCountBonus);
    }
    
    public struct WeaponDamage : IComponentData
    {
        public float BaseDamage;
        public float DamageBonus; 
        public float TotalDamage => BaseDamage + (DamageBonus/100f * DamageBonus);
    }

    public struct WeaponSpeed : IComponentData
    {
        public float BaseSpeed;
        public float SpeedBonus; 
        public float TotalSpeed => BaseSpeed + SpeedBonus/100f * SpeedBonus;
    }
    
    public struct WeaponFireRate : IComponentData
    {
        public float BaseInterval;
        public float IntervalReductionBonus;
        public float CurrentRate;
        public float TotalFireRate => BaseInterval - IntervalReductionBonus/100f * IntervalReductionBonus;
    }
}
