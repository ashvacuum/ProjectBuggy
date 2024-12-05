using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace NonECS.BaseWeapons
{
    public class WeaponBase : UnityEngine.ScriptableObject
    {
        public List<UpgradeInfo> upgradeData = new List<UpgradeInfo>();
    }

    [Serializable]
    public struct UpgradeInfo
    {
        public WeaponUpgradeType UpgradeType;
        public float Lifetime;
        public int Count;
        public int Penetration;
        public float Speed;
        public float FireRate;
        public float WeaponSize;
        public float Damage;
        public float Knockback;
        public float Range;
        public float Critical;
    }
    
    public enum WeaponClass
    {
        Projectile = 1 << 0,
        Laser = 1 << 1,
        Artillery = 1 << 2
    }
    [Flags]
    public enum WeaponUpgradeType
    {
        Lifetime = 1 << 0,
        Count = 1 << 1,
        Penetration = 1 << 2,
        Speed = 1 << 3,
        FireRate = 1 << 4,
        Size = 1 << 5,
        Damage = 1 << 6,
        Knockback = 1 << 7,
        Range = 1 << 8
    }

    public struct ProjectileTag : IComponentData { }
    public struct LaserTag : IComponentData { }
    public struct ArtilleryTag : IComponentData { }
    
    
}


