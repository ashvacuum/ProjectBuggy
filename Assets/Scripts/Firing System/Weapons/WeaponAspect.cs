
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
public readonly partial struct WeaponAspect : IAspect
{
    readonly RefRO<WeaponData> m_Weapon;
    
    public Entity BulletPrefab => m_Weapon.ValueRO.bulletPrefab;
    public Entity BulletSpawn => m_Weapon.ValueRO.bulletSpawn;

    public int Id => m_Weapon.ValueRO.Id;
    public int NameId => m_Weapon.ValueRO.NameId;
    public float AreaOfEffect => m_Weapon.ValueRO.AreaOfEffect;
    public float Duration => m_Weapon.ValueRO.Duration;
    public int NumProjectiles => m_Weapon.ValueRO.NumProjectiles;
    public float DamagePerProjectile => m_Weapon.ValueRO.DamagePerProjectile;
    public float ProjectileSpeed => m_Weapon.ValueRO.ProjectileSpeed;
    public int ProjectilePenetration => m_Weapon.ValueRO.ProjectilePenetration;
    public float ProjectileInterval => m_Weapon.ValueRO.ProjectileInterval;
}
