using System.Collections.Generic;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    public static class WeaponTemplates
    {
        public static WeaponDefinition CreateProjectileWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "Projectile";
            weapon.displayName = "Single Target Bullet";
            weapon.description = "Fires bullets at enemies. Each upgrade increases the number of bullets fired.";
            weapon.category = WeaponCategory.Projectile;
            weapon.archetype = WeaponArchetype.SingleTarget;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 10f,
                baseFireRate = 1f,
                baseNumProjectiles = 1,
                baseRange = 15f,
                baseSpeed = 10f,
                baseLifetime = 3f,
                baseSize = 1f,
                basePenetration = 1,
                baseKnockback = 2f,
                baseCritical = 0.05f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.AutoTarget | WeaponBehaviorFlags.DestroyOnContact;
            
            // Create upgrade levels matching your design doc
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "Fires 2 bullets",
                    statAdditions = new WeaponStats { baseNumProjectiles = 1 },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Fires 3 bullets",
                    statAdditions = new WeaponStats { baseNumProjectiles = 2 },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "Fires 4 bullets, increased damage",
                    statAdditions = new WeaponStats { baseNumProjectiles = 3 },
                    statMultipliers = new WeaponStats { baseDamage = 1.2f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 4,
                    description = "Fires 5 bullets, increased damage",
                    statAdditions = new WeaponStats { baseNumProjectiles = 4 },
                    statMultipliers = new WeaponStats { baseDamage = 1.4f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 5,
                    description = "Fires 6 bullets, increased damage",
                    statAdditions = new WeaponStats { baseNumProjectiles = 5 },
                    statMultipliers = new WeaponStats { baseDamage = 1.6f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 6,
                    description = "Fires 7 bullets, increased damage",
                    statAdditions = new WeaponStats { baseNumProjectiles = 6 },
                    statMultipliers = new WeaponStats { baseDamage = 1.8f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 7,
                    description = "FINAL UPGRADE: Fires bullets in all directions that pierce through enemies",
                    statAdditions = new WeaponStats { baseNumProjectiles = 6, basePenetration = 99 },
                    statMultipliers = new WeaponStats { baseDamage = 2f },
                    specialEffects = new List<string> { "OmnidirectionalFire", "InfinitePierce" }
                }
            };
            
            weapon.unlockRequirements = new List<UpgradeRequirement>
            {
                new UpgradeRequirement
                {
                    requiredUpgradeType = "FireRateReductionBonus",
                    minimumLevel = 1,
                    description = "Requires Fire Rate ship system to reach max level"
                }
            };
            
            return weapon;
        }
        
        public static WeaponDefinition CreateArtilleryWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "Artillery";
            weapon.displayName = "Artillery Barrage";
            weapon.description = "Fires a barrage of missiles towards a direction. Creates explosions on impact.";
            weapon.category = WeaponCategory.Artillery;
            weapon.archetype = WeaponArchetype.AreaOfEffect;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 25f,
                baseFireRate = 0.5f,
                baseNumProjectiles = 3,
                baseRange = 20f,
                baseSpeed = 8f,
                baseLifetime = 2.5f,
                baseSize = 2f,
                basePenetration = 1,
                baseKnockback = 5f,
                baseCritical = 0.1f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.ExplodeOnImpact | WeaponBehaviorFlags.AimAtCursor;
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "Increased missile count and damage",
                    statAdditions = new WeaponStats { baseNumProjectiles = 1 },
                    statMultipliers = new WeaponStats { baseDamage = 1.2f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Even more missiles, larger explosions",
                    statAdditions = new WeaponStats { baseNumProjectiles = 2 },
                    statMultipliers = new WeaponStats { baseDamage = 1.4f, baseSize = 1.3f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "Maximum missiles, increased explosion size",
                    statAdditions = new WeaponStats { baseNumProjectiles = 3 },
                    statMultipliers = new WeaponStats { baseDamage = 1.6f, baseSize = 1.6f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 4,
                    description = "FINAL UPGRADE: Creates persistent damage zones, fires on both sides",
                    statAdditions = new WeaponStats { baseNumProjectiles = 4 },
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseSize = 2f, baseLifetime = 3f },
                    specialEffects = new List<string> { "PersistentDamageZone", "DualSideFiring" }
                }
            };
            
            weapon.unlockRequirements = new List<UpgradeRequirement>
            {
                new UpgradeRequirement
                {
                    requiredUpgradeType = "SizeBonus",
                    minimumLevel = 1,
                    description = "Requires Area/Size ship upgrade module"
                }
            };
            
            return weapon;
        }
        
        public static WeaponDefinition CreateLaserWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "LaserStrike";
            weapon.displayName = "Laser Strikes";
            weapon.description = "Fires lasers that target enemies in range. Fast firing, low damage. Destroyed upon hitting an enemy.";
            weapon.category = WeaponCategory.Laser;
            weapon.archetype = WeaponArchetype.SingleTarget;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 5f,
                baseFireRate = 3f,
                baseNumProjectiles = 1,
                baseRange = 12f,
                baseSpeed = 15f,
                baseLifetime = 1f,
                baseSize = 0.5f,
                basePenetration = 1,
                baseKnockback = 0.5f,
                baseCritical = 0.15f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.AutoTarget | WeaponBehaviorFlags.DestroyOnContact;
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "Increased fire rate",
                    statMultipliers = new WeaponStats { baseFireRate = 1.5f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Much faster firing, resembles gatling gun",
                    statMultipliers = new WeaponStats { baseFireRate = 2f, baseDamage = 1.2f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "Maximum fire rate, increased damage",
                    statMultipliers = new WeaponStats { baseFireRate = 3f, baseDamage = 1.5f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 4,
                    description = "FINAL UPGRADE: Fires continuous beam, damage over time",
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseLifetime = 5f },
                    specialEffects = new List<string> { "ContinuousBeam", "DamageOverTime", "EnergyGathering" }
                }
            };
            
            weapon.unlockRequirements = new List<UpgradeRequirement>
            {
                new UpgradeRequirement
                {
                    requiredUpgradeType = "DamageBonus",
                    minimumLevel = 1,
                    description = "Requires Damage Per Projectile upgrade"
                }
            };
            
            return weapon;
        }
    }
}