using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    [CreateAssetMenu(fileName = "New Weapon Definition", menuName = "Weapons/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName = "NewWeapon";
        public string displayName = "New Weapon";
        [TextArea(3, 5)]
        public string description = "A new weapon system";
        
        [Header("Weapon Type")]
        public WeaponCategory category = WeaponCategory.Projectile;
        public WeaponArchetype archetype = WeaponArchetype.SingleTarget;
        
        [Header("Base Stats")]
        public WeaponStats baseStats = new WeaponStats();
        
        [Header("Upgrade Progression")]
        public List<WeaponUpgradeLevel> upgradeLevels = new List<WeaponUpgradeLevel>();
        
        [Header("Visual & Audio")]
        public GameObject projectilePrefab;
        public GameObject muzzleFlashPrefab;
        public GameObject impactEffectPrefab;
        public AudioClip fireSound;
        public AudioClip impactSound;
        
        [Header("Advanced Settings")]
        public WeaponBehaviorFlags behaviorFlags = WeaponBehaviorFlags.None;
        public List<WeaponModifier> specialModifiers = new List<WeaponModifier>();
        
        [Header("Complex Behaviors")]
        public List<WeaponBehaviorDefinition> complexBehaviors = new List<WeaponBehaviorDefinition>();
        
        [Header("Requirements")]
        public List<UpgradeRequirement> unlockRequirements = new List<UpgradeRequirement>();
        
        // Generate component name for ECS
        public string GetComponentName() => $"{weaponName}Attack";
        
        // Generate class name for weapon base
        public string GetWeaponBaseName() => $"{weaponName}WeaponBase";
        
        // Get upgrade type enum name
        public string GetUpgradeTypeName() => weaponName;
    }
    
    [Serializable]
    public struct WeaponStats
    {
        [Header("Damage")]
        public float baseDamage;
        public float baseCritical;
        public float baseKnockback;
        
        [Header("Firing")]
        public float baseFireRate;
        public int baseNumProjectiles;
        public float baseRange;
        
        [Header("Projectile")]
        public float baseSpeed;
        public float baseLifetime;
        public float baseSize;
        public int basePenetration;
        
        public static WeaponStats Default => new WeaponStats
        {
            baseDamage = 10f,
            baseCritical = 0f,
            baseKnockback = 1f,
            baseFireRate = 1f,
            baseNumProjectiles = 1,
            baseRange = 10f,
            baseSpeed = 5f,
            baseLifetime = 2f,
            baseSize = 1f,
            basePenetration = 1
        };
    }
    
    [Serializable]
    public struct WeaponUpgradeLevel
    {
        public int level;
        public WeaponStats statMultipliers; // Multipliers to apply to base stats
        public WeaponStats statAdditions;   // Flat additions to base stats
        public List<string> specialEffects; // Special effects at this level
        
        [TextArea(2, 3)]
        public string description;
    }
    
    [Serializable]
    public struct WeaponModifier
    {
        public string name;
        public WeaponModifierType type;
        public float value;
        public string description;
    }
    
    [Serializable]
    public struct UpgradeRequirement
    {
        public string requiredUpgradeType;
        public int minimumLevel;
        public string description;
    }
    
    public enum WeaponCategory
    {
        Projectile,
        Artillery,
        Laser,
        Beam,
        Missile,
        Drone,
        Special
    }
    
    public enum WeaponArchetype
    {
        SingleTarget,
        AreaOfEffect,
        Piercing,
        Homing,
        Continuous,
        Burst,
        Orbital,
        Defensive
    }
    
    [Flags]
    public enum WeaponBehaviorFlags
    {
        None = 0,
        HomingTarget = 1 << 0,
        PierceEnemies = 1 << 1,
        ExplodeOnImpact = 1 << 2,
        ChainToNearby = 1 << 3,
        IgnoreArmor = 1 << 4,
        DestroyOnContact = 1 << 5,
        ContinuousFire = 1 << 6,
        RequiresChargeUp = 1 << 7,
        AimAtCursor = 1 << 8,
        AutoTarget = 1 << 9
    }
    
    public enum WeaponModifierType
    {
        DamageMultiplier,
        RangeMultiplier,
        FireRateMultiplier,
        CriticalChance,
        LifetimeMultiplier,
        SizeMultiplier,
        SpeedMultiplier,
        PenetrationBonus,
        KnockbackMultiplier,
        ProjectileCountBonus
    }
}