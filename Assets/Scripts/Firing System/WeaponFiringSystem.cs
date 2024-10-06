using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class WeaponFiringSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

    protected override void OnCreate()
    {
        _commandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var commandBuffer = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        // Create a local variable to hold the command buffer for use in the lambda
        var weaponDataList = new NativeList<WeaponData>(Allocator.TempJob);

        Entities
            .WithAll<WeaponComponent, ProximityComponent>()
            .ForEach((Entity entity, int entityInQueryIndex, ref WeaponComponent weapon, ref ProximityComponent proximity, in LocalToWorld weaponPosition) =>
            {
                if (proximity.EnemyInProximity)
                {
                    // Iterate over each equipped weapon
                    for (int i = 0; i < weapon.EquippedWeapons.Length; i++)
                    {
                        var weaponData = weapon.EquippedWeapons[i];

                        // Check if the weapon can fire based on FireRate
                        if (CanFireWeapon(weaponData, deltaTime))
                        {
                            FireProjectiles(commandBuffer, entityInQueryIndex, weaponData, weaponPosition);
                        }
                    }
                }
            }).ScheduleParallel();

        _commandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    private bool CanFireWeapon(WeaponData weaponData, float deltaTime)
    {
        // Implement logic to check if the weapon can fire based on FireRate and elapsed time
        // For demonstration, let's assume we always allow firing
        return true;
    }

    private void FireProjectiles(EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, WeaponData weaponData, LocalToWorld weaponPosition)
    {
        for (int j = 0; j < weaponData.NumProjectiles; j++)
        {
            // Logic to instantiate projectiles based on weapon data
            // You will need to create a new projectile entity and set its properties based on weaponData
            // Example:
            // var projectileEntity = ecb.Instantiate(entityInQueryIndex, projectilePrefab);
            // ecb.SetComponent(entityInQueryIndex, projectileEntity, new ProjectileData { Speed = weaponData.ProjectileSpeed, Damage = weaponData.DamagePerProjectile });
            // Set the position and direction of the projectile based on weaponPosition and firing direction
        }
    }
}
