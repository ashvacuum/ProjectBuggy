using Authoring;
using Unity.Entities;
using Unity.Transforms;

namespace Firing_System
{
    public partial struct  WeaponShootingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Weapon>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var weapon in
                     SystemAPI.Query<WeaponAspect>()
                         .WithAll<Shooting>())
            {
                if (weapon.BulletPrefab == Entity.Null) continue;
                var instance = state.EntityManager.Instantiate(weapon.BulletPrefab);
                state.EntityManager.AddComponent<DamageComponent>(instance);
                state.EntityManager.SetComponentData(instance, new DamageComponent()
                {
                    Damage = weapon.DamagePerProjectile
                });
                state.EntityManager.AddComponent<HealthComponent>(instance);
                state.EntityManager.SetComponentData(instance, new HealthComponent()
                {
                    CurrentHealth = weapon.ProjectilePenetration,
                    CurrentNextTimeToTakeDamage = 0,
                    MaxHealth = weapon.ProjectilePenetration,
                    NextTimeToTakeDamage = 0f,
                    PreviousHealth = weapon.ProjectilePenetration
                });
                state.EntityManager.SetComponentData(instance, new LocalTransform
                {
                    Position = SystemAPI.GetComponent<LocalToWorld>(weapon.BulletSpawn).Position,
                    Rotation = SystemAPI.GetComponent<LocalToWorld>(weapon.BulletSpawn).Rotation,
                    Scale = SystemAPI.GetComponent<LocalTransform>(weapon.BulletPrefab).Scale
                });

                state.EntityManager.SetComponentData(instance, new Projectile
                {
                    Velocity = weapon.LocalToWorld.ValueRO.Right * 20.0f
                });


            }
        }
    }
}
