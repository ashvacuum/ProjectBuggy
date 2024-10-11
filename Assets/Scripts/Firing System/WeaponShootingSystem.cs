using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public partial struct  WeaponShootingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Weapon>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (weapon, localToWorld) in
                 SystemAPI.Query<WeaponAspect, RefRO<LocalToWorld>>()
                     .WithAll<Shooting>())
        {
            if (weapon.BulletPrefab == Entity.Null) continue;
            var instance = state.EntityManager.Instantiate(weapon.BulletPrefab);

            state.EntityManager.SetComponentData(instance, new LocalTransform
            {
                Position = SystemAPI.GetComponent<LocalToWorld>(weapon.BulletSpawn).Position,
                Rotation = SystemAPI.GetComponent<LocalToWorld>(weapon.BulletSpawn).Rotation,
                Scale = SystemAPI.GetComponent<LocalTransform>(weapon.BulletPrefab).Scale
            });

            state.EntityManager.SetComponentData(instance, new Projectile
            {
                Velocity = localToWorld.ValueRO.Right * 20.0f
            });


        }
    }
}
