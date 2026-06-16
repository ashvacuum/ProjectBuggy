using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Firing_System
{
    public partial struct WeaponShootingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Weapon>();
        }

        public void OnUpdate(ref SystemState state)
        {

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (weaponData, localToWorld) in
                     SystemAPI.Query<RefRO<WeaponData>, RefRW<LocalToWorld>>()
                         .WithAll<Shooting>())
            {
                var weapon = new WeaponAspect(weaponData, localToWorld);
                //if (weapon.BulletPrefab == Entity.Null) continue;
                var instance = state.EntityManager.Instantiate(weapon.BulletPrefab);
                ecb.AddComponent<DamageComponent>(instance);
                ecb.SetComponent(instance, new DamageComponent()
                {
                    Damage = weapon.DamagePerProjectile
                });
                ecb.AddComponent<HealthComponent>(instance);
                ecb.SetComponent(instance, new HealthComponent()
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

                ecb.SetComponent(instance, new Projectile
                {
                    Velocity = weapon.LocalToWorld.ValueRO.Right * 20.0f
                });


            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

    }
}
