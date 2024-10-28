using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    
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

            foreach (var (transform, weapon) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ProjectileAttack>>()
                         .WithAll<PlayerTag>())
            {

                if (weapon.ValueRO.CurrentFireRate > 0)
                {
                    weapon.ValueRW.CurrentFireRate -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                weapon.ValueRW.CurrentFireRate = weapon.ValueRO.TotalFireRate;
                var totalCountWeapons = math.min(enemyTargetBuffers.Length, weapon.ValueRO.TotalCount);
                for (var i = 0; i < totalCountWeapons; i++)
                {
                    var instance = ecb.Instantiate(projectileSpawnerData.ProjectileToSpawn);
                    ecb.AddComponent(instance, new DamageComponent()
                    {
                        Damage = weapon.ValueRO.TotalDamage
                    });
                    
                    var targetPos = enemyTargetBuffers[i].Position;
                    
                    var direction = math.normalize(targetPos - transform.ValueRO.Position);
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
                        Scale = weapon.ValueRO.TotalSize
                    });
                    ecb.AddComponent(instance, new KnockbackSender()
                    {
                        knockbackForceToSend = weapon.ValueRO.TotalKnockback
                    });

                }

            }
            enemyTargetBuffers.Clear();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    
    
    public struct ProjectileAttack  : IComponentData, IEnableableComponent
    {
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

        public float BaseKnockback;
        public float KnockbackBonus;
        public float TotalKnockback => BaseKnockback - KnockbackBonus/100f * BaseKnockback;
        
        
        public float CurrentFireRate; // value to edit if it hits 0 it will fire and reset to total fire rate
    }
}
