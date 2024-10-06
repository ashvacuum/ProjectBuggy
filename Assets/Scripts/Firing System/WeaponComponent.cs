using Unity.Entities;
using UnityEngine;

public struct WeaponComponent : IComponentData
{
    public float FireRate;
    public float AreaOfEffect;
    public float Duration;
    public int NumProjectiles;
    public float DamagePerProjectile;
    public float ProjectileSpeed;
    public int ProjectilePenetration;
}

