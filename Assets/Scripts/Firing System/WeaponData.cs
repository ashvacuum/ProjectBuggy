using Unity.Entities;

public struct WeaponData : IComponentData
{
    public Entity bulletPrefab;
    public Entity bulletSpawn;
    public int Id;
    public int NameId;
    public float AreaOfEffect;
    public float Duration;
    public int NumProjectiles;
    public float DamagePerProjectile;
    public float ProjectileSpeed;
    public int ProjectilePenetration;
    public float ProjectileInterval;
}   

public struct Shooting : IComponentData, IEnableableComponent
{
}

public struct Weapon : IComponentData
{
    
}