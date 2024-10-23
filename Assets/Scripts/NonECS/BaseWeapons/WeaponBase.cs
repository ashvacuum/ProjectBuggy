using Unity.Entities;
using UnityEngine;

namespace NonECS.BaseWeapons
{
    public class WeaponBase : ScriptableObject
    {
        public float BaseLifetime = 5f;
        public int BaseCount = 1;
        public int BasePenetration = 0;
        public float BaseSpeed = 10;
        public float BaseFireRate = 3f;
        public float WeaponSize = 1f;
    }
    
    public enum WeaponClass
    {
        Projectile = 1 << 0,
        Laser = 1 << 1,
        Artillery = 1 << 2
    }

    public struct ProjectileTag : IComponentData { }
    public struct LaserTag : IComponentData { }
    public struct ArtilleryTag : IComponentData { }
    
    
}


