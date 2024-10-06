using Unity.Entities;
using UnityEngine;

public class WeaponComponentAuthoring : MonoBehaviour
{
    public float FireRate;
    public float AreaOfEffect;
    public float Duration;
    public int NumProjectiles;
    public float DamagePerProjectile;
    public float ProjectileSpeed;
    public int ProjectilePenetration;

    public class Baker : Baker<WeaponComponentAuthoring>
    {
        public override void Bake(WeaponComponentAuthoring authoring)
        {
            var weaponComponent = new WeaponComponent
            {
                FireRate = authoring.FireRate,
                AreaOfEffect = authoring.AreaOfEffect,
                Duration = authoring.Duration,
                NumProjectiles = authoring.NumProjectiles,
                DamagePerProjectile = authoring.DamagePerProjectile,
                ProjectileSpeed = authoring.ProjectileSpeed,
                ProjectilePenetration = authoring.ProjectilePenetration
            };
            
            AddComponent(weaponComponent);
        }
    }
}