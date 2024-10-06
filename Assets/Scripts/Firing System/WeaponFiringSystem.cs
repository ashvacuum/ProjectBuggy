using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

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
        var elapsedTime = SystemAPI.Time.ElapsedTime;
        var commandBuffer = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        var ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities
            .WithAll<WeaponComponent, LaserComponent>()
            .ForEach((Entity entity, int entityInQueryIndex, ref WeaponComponent weapon, ref LaserComponent laser, ref TargetComponent target, ref ProximityComponent proximity, in LocalToWorld weaponPosition) =>
            {
                if (proximity.EnemyInProximity && target.HasTarget)
                {
                    float3 direction = math.normalize(target.TargetPosition - weaponPosition.Position);
                    float currentTime = (float)elapsedTime;

                    if (currentTime >= laser.LastFireTime + laser.BarrelOffsetTime && laser.IsFiring)
                    {
                        FireProjectile(ecb, entityInQueryIndex, weapon, direction);
                        laser.LastFireTime = currentTime;
                    }

                    if (weapon.NumProjectiles == 0)
                    {
                        laser.IsFiring = false;
                    }
                }
            }).ScheduleParallel();
        
        _commandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    private static void FireProjectile(EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, WeaponComponent weapon, float3 direction)
    {
        // Add logic to create projectile entities with the provided attributes like speed, damage, etc.
    }
}
