using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class WeaponInitialization : MonoBehaviour
{
    private EntityManager entityManager;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create weapon entity
        Entity weaponEntity = entityManager.CreateEntity(
            typeof(WeaponComponent),
            typeof(LaserComponent),
            typeof(TargetComponent),
            typeof(ProximityComponent),
            typeof(LocalToWorld) // Assuming some position in the world
        );

        // Initialize components
        entityManager.SetComponentData(weaponEntity, new WeaponComponent
        {
            FireRate = 1f,
            AreaOfEffect = 5f,
            Duration = 10f,
            NumProjectiles = 100,
            DamagePerProjectile = 10f,
            ProjectileSpeed = 50f,
            ProjectilePenetration = 1
        });

        entityManager.SetComponentData(weaponEntity, new LaserComponent
        {
            BarrelOffsetTime = 0.05f,
            IsFiring = true,
            LastFireTime = 0f
        });

        entityManager.SetComponentData(weaponEntity, new ProximityComponent
        {
            ProximityRadius = 10f,
            EnemyInProximity = false
        });
    }
}