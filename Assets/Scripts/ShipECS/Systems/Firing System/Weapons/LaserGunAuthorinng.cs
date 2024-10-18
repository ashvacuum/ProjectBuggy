using UnityEngine;
using Unity.Entities;

public class LaserGunAuthoring : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    
    public int Id;
    public int NameId;
    public float AreaOfEffect;
    public float Duration;
    public int NumProjectiles;
    public float DamagePerProjectile;
    public float ProjectileSpeed;
    public int ProjectilePenetration;
    public float ProjectileInterval;
    
    class Baker : Baker<LaserGunAuthoring>
    {
        public override void Bake(LaserGunAuthoring authoring)
        {
            
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new WeaponData
            {
                bulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                bulletSpawn = GetEntity(authoring.bulletSpawn, TransformUsageFlags.Dynamic),
                
                Id = authoring.Id,
                NameId = authoring.NameId,
                AreaOfEffect = authoring.AreaOfEffect,
                Duration = authoring.Duration,
                NumProjectiles = authoring.NumProjectiles,
                DamagePerProjectile = authoring.DamagePerProjectile,
                ProjectileSpeed = authoring.ProjectileSpeed,
                ProjectilePenetration = authoring.ProjectilePenetration,
                ProjectileInterval = authoring.ProjectileInterval
            });
            
            AddComponent<Shooting>(entity);
            AddComponent<Weapon>(entity);
        }
    }
}