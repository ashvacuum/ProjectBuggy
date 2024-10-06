using Unity.Entities;
using Unity.Collections;

public struct WeaponData : IComponentData
{
    public int Id; // Unique identifier for the weapon
    public string Name; // Name of the weapon
    public float FireRate; // Time in seconds between shots
    public float AreaOfEffect; // AOE radius
    public float Duration; // Duration for which the weapon is active
    public int NumProjectiles; // Number of projectiles fired per shot
    public float DamagePerProjectile; // Damage dealt per projectile
    public float ProjectileSpeed; // Speed of the projectile
    public int ProjectilePenetration; // How many times a projectile can hit before disappearing
}
public struct WeaponComponent : IComponentData
{
    public NativeList<WeaponData> EquippedWeapons; // List of equipped weapons
    public int ActiveWeaponIndex; // Index of the currently active weapon
}

