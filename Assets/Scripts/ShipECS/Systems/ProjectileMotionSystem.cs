using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct ProjectileMotionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProjectileMotion>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (projectile, health,transform) in SystemAPI.Query<RefRW<ProjectileMotion>,RefRO<HealthComponent>, RefRW<LocalTransform>>())
            {
                if (!(projectile.ValueRO.LifeTime > 0) || health.ValueRO.CurrentHealth <= 0) continue;
                
                transform.ValueRW.Position = math.lerp(transform.ValueRO.Position,
                    transform.ValueRO.Position + projectile.ValueRO.Direction,
                    Time.deltaTime * projectile.ValueRO.Speed);
                projectile.ValueRW.LifeTime -= SystemAPI.Time.DeltaTime;
            }
        }
    }

    public struct ProjectileMotion : IComponentData, IEnableableComponent
    {
        public float3 Direction;
        public float Speed;
        public float LifeTime;
    }
    
    public readonly partial struct ProjectileWeaponAspect : IAspect
    {
        public readonly RefRO<WeaponLifetime> Lifetime;
        public readonly RefRO<WeaponCount> NumProjectiles;
        public readonly RefRO<WeaponPenetration> Penetration;
        public readonly RefRO<WeaponSpeed> Speed;
        public readonly RefRO<WeaponDamage> Damage;
        public readonly RefRW<WeaponFireRate> FireRate;
        public readonly RefRO<LocalTransform> LocalTransform;
        readonly RefRO<ProjectileAttack> _weapon;
        public Entity Projectile => _weapon.ValueRO.ProjectilePrefab;

    }
}


