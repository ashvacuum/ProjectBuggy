using System.Collections.Generic;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    public static class ExampleComplexWeapons
    {
        public static WeaponDefinition CreateBeamWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "PlasmaBeam";
            weapon.displayName = "Plasma Beam Cannon";
            weapon.description = "Charges up then fires a continuous beam that pierces through all enemies";
            weapon.category = WeaponCategory.Beam;
            weapon.archetype = WeaponArchetype.Continuous;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 50f,        // High damage per second
                baseFireRate = 0.3f,     // Slow charge rate
                baseNumProjectiles = 1,
                baseRange = 25f,         // Long range
                baseSpeed = 0f,          // Instant beam
                baseLifetime = 3f,       // Beam duration
                baseSize = 2f,           // Beam width
                basePenetration = 99,    // Infinite penetration
                baseKnockback = 0f,      // No knockback
                baseCritical = 0.2f
            };
            
            // Standard behavior flags
            weapon.behaviorFlags = WeaponBehaviorFlags.RequiresChargeUp | 
                                 WeaponBehaviorFlags.ContinuousFire | 
                                 WeaponBehaviorFlags.PierceEnemies |
                                 WeaponBehaviorFlags.AimAtCursor;
            
            // Complex behavior definition
            var beamBehavior = new WeaponBehaviorDefinition
            {
                behaviorType = WeaponBehaviorType.ContinuousBeam,
                customSystemName = "BeamWeaponSystem",
                requiredComponents = new List<string> 
                { 
                    "BeamWeaponComponent", 
                    "EnergyComponent",
                    "ChargeComponent" 
                },
                parameters = new List<BehaviorParameter>
                {
                    new BehaviorParameter { name = "beamWidth", value = "2.0", description = "Width of the beam" },
                    new BehaviorParameter { name = "beamLength", value = "25.0", description = "Maximum beam length" },
                    new BehaviorParameter { name = "chargeTime", value = "1.5", description = "Time to charge before firing" },
                    new BehaviorParameter { name = "sustainTime", value = "3.0", description = "How long beam lasts" },
                    new BehaviorParameter { name = "energyConsumption", value = "20.0", description = "Energy consumed per second" },
                    new BehaviorParameter { name = "pierceEnemies", value = "true", description = "Can hit multiple enemies" },
                    new BehaviorParameter { name = "damageTickRate", value = "0.1", description = "Damage applied every X seconds" }
                }
            };
            
            weapon.complexBehaviors = new List<WeaponBehaviorDefinition> { beamBehavior };
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "Reduced charge time, increased damage",
                    statMultipliers = new WeaponStats { baseDamage = 1.3f },
                    statAdditions = new WeaponStats { baseFireRate = 0.2f }, // Faster charge
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Wider beam, longer duration",
                    statMultipliers = new WeaponStats { baseDamage = 1.6f, baseSize = 1.5f, baseLifetime = 1.5f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "FINAL: Dual beams, no charge time required",
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseSize = 2f },
                    statAdditions = new WeaponStats { baseNumProjectiles = 1, baseFireRate = 2f },
                    specialEffects = new List<string> { "DualBeams", "InstantFire", "EnergyGathering" }
                }
            };
            
            return weapon;
        }
        
        public static WeaponDefinition CreateChainLightningWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "ChainLightning";
            weapon.displayName = "Arc Lightning";
            weapon.description = "Fires lightning that chains between nearby enemies, dealing reduced damage with each chain";
            weapon.category = WeaponCategory.Laser;
            weapon.archetype = WeaponArchetype.Piercing;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 30f,
                baseFireRate = 1.5f,
                baseNumProjectiles = 1,
                baseRange = 15f,
                baseSpeed = 0f,           // Instant
                baseLifetime = 0.5f,      // Visual effect duration
                baseSize = 1f,
                basePenetration = 5,      // Max chains
                baseKnockback = 3f,
                baseCritical = 0.25f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.ChainToNearby | WeaponBehaviorFlags.AutoTarget;
            
            var chainBehavior = new WeaponBehaviorDefinition
            {
                behaviorType = WeaponBehaviorType.ChainLightning,
                customSystemName = "ChainLightningSystem",
                requiredComponents = new List<string> { "ChainLightningComponent", "TargetChainBuffer" },
                parameters = new List<BehaviorParameter>
                {
                    new BehaviorParameter { name = "maxChains", value = "3", description = "Maximum number of chain targets" },
                    new BehaviorParameter { name = "chainRange", value = "8.0", description = "Range to find chain targets" },
                    new BehaviorParameter { name = "damageReductionPerChain", value = "0.7", description = "Damage multiplier per chain" },
                    new BehaviorParameter { name = "chainDelay", value = "0.1", description = "Delay between chain jumps" },
                    new BehaviorParameter { name = "canChainToSameTarget", value = "false", description = "Can hit same enemy multiple times" }
                }
            };
            
            weapon.complexBehaviors = new List<WeaponBehaviorDefinition> { chainBehavior };
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "More chains, less damage reduction",
                    statAdditions = new WeaponStats { basePenetration = 1 }, // +1 chain
                    statMultipliers = new WeaponStats { baseDamage = 1.2f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Even more chains, increased range",
                    statAdditions = new WeaponStats { basePenetration = 2, baseRange = 5f },
                    statMultipliers = new WeaponStats { baseDamage = 1.5f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "FINAL: Chains can hit same target, creates chain explosions",
                    statAdditions = new WeaponStats { basePenetration = 3 },
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseRange = 1.5f },
                    specialEffects = new List<string> { "ChainExplosions", "RecursiveChaining", "ElectricField" }
                }
            };
            
            return weapon;
        }
        
        public static WeaponDefinition CreateHomingMissileWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "HomingMissile";
            weapon.displayName = "Smart Missiles";
            weapon.description = "Fires missiles that track enemies and explode on impact";
            weapon.category = WeaponCategory.Missile;
            weapon.archetype = WeaponArchetype.Homing;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 40f,
                baseFireRate = 0.8f,
                baseNumProjectiles = 2,
                baseRange = 20f,
                baseSpeed = 8f,
                baseLifetime = 5f,
                baseSize = 3f,           // Explosion radius
                basePenetration = 1,
                baseKnockback = 8f,
                baseCritical = 0.1f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.HomingTarget | 
                                 WeaponBehaviorFlags.ExplodeOnImpact |
                                 WeaponBehaviorFlags.AutoTarget;
            
            var homingBehavior = new WeaponBehaviorDefinition
            {
                behaviorType = WeaponBehaviorType.HomingMissile,
                customSystemName = "HomingMissileSystem",
                requiredComponents = new List<string> { "HomingComponent", "TargetEntity", "ExplosionComponent" },
                parameters = new List<BehaviorParameter>
                {
                    new BehaviorParameter { name = "trackingSpeed", value = "180.0", description = "Degrees per second turn rate" },
                    new BehaviorParameter { name = "maxTrackingRange", value = "20.0", description = "Maximum distance to track target" },
                    new BehaviorParameter { name = "accelerationTime", value = "0.5", description = "Time to reach max speed" },
                    new BehaviorParameter { name = "retargetOnKill", value = "true", description = "Find new target if current dies" },
                    new BehaviorParameter { name = "explosionRadius", value = "3.0", description = "Explosion damage radius" }
                }
            };
            
            weapon.complexBehaviors = new List<WeaponBehaviorDefinition> { homingBehavior };
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "More missiles, better tracking",
                    statAdditions = new WeaponStats { baseNumProjectiles = 1 },
                    statMultipliers = new WeaponStats { baseDamage = 1.2f, baseSpeed = 1.3f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Cluster missiles, larger explosions",
                    statAdditions = new WeaponStats { baseNumProjectiles = 2 },
                    statMultipliers = new WeaponStats { baseDamage = 1.5f, baseSize = 1.5f },
                    specialEffects = new List<string> { "ClusterMissiles" }
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "FINAL: Missiles split on impact, creating submunitions",
                    statAdditions = new WeaponStats { baseNumProjectiles = 3 },
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseSize = 2f },
                    specialEffects = new List<string> { "SplitMissiles", "Submunitions", "ChainExplosions" }
                }
            };
            
            return weapon;
        }
        
        public static WeaponDefinition CreateDroneSwarmWeapon()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.weaponName = "DroneSwarm";
            weapon.displayName = "Combat Drones";
            weapon.description = "Spawns autonomous combat drones that follow and protect the player";
            weapon.category = WeaponCategory.Drone;
            weapon.archetype = WeaponArchetype.Orbital;
            
            weapon.baseStats = new WeaponStats
            {
                baseDamage = 15f,        // Per drone
                baseFireRate = 3f,       // Drone spawn rate
                baseNumProjectiles = 3,  // Max drones
                baseRange = 12f,         // Drone attack range
                baseSpeed = 10f,         // Drone movement speed
                baseLifetime = 15f,      // Drone lifetime
                baseSize = 1f,
                basePenetration = 1,
                baseKnockback = 2f,
                baseCritical = 0.05f
            };
            
            weapon.behaviorFlags = WeaponBehaviorFlags.AutoTarget;
            
            var swarmBehavior = new WeaponBehaviorDefinition
            {
                behaviorType = WeaponBehaviorType.DroneSwarm,
                customSystemName = "DroneSwarmSystem",
                requiredComponents = new List<string> { "DroneSpawnerComponent", "DroneBuffer", "FollowComponent" },
                parameters = new List<BehaviorParameter>
                {
                    new BehaviorParameter { name = "maxDrones", value = "3", description = "Maximum active drones" },
                    new BehaviorParameter { name = "droneLifetime", value = "15.0", description = "How long drones last" },
                    new BehaviorParameter { name = "droneSpeed", value = "10.0", description = "Drone movement speed" },
                    new BehaviorParameter { name = "droneDamage", value = "15.0", description = "Damage per drone attack" },
                    new BehaviorParameter { name = "spawnDelay", value = "2.0", description = "Delay between drone spawns" },
                    new BehaviorParameter { name = "followDistance", value = "5.0", description = "Distance drones orbit player" }
                }
            };
            
            weapon.complexBehaviors = new List<WeaponBehaviorDefinition> { swarmBehavior };
            
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>
            {
                new WeaponUpgradeLevel
                {
                    level = 1,
                    description = "More drones, longer lifetime",
                    statAdditions = new WeaponStats { baseNumProjectiles = 1, baseLifetime = 5f },
                    statMultipliers = new WeaponStats { baseDamage = 1.2f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 2,
                    description = "Even more drones, they move faster",
                    statAdditions = new WeaponStats { baseNumProjectiles = 1 },
                    statMultipliers = new WeaponStats { baseDamage = 1.5f, baseSpeed = 1.5f },
                    specialEffects = new List<string>()
                },
                new WeaponUpgradeLevel
                {
                    level = 3,
                    description = "FINAL: Drones explode on death, spawn mini-drones",
                    statAdditions = new WeaponStats { baseNumProjectiles = 2 },
                    statMultipliers = new WeaponStats { baseDamage = 2f, baseLifetime = 2f },
                    specialEffects = new List<string> { "ExplodeOnDeath", "SpawnMiniDrones", "RegenerativeDrones" }
                }
            };
            
            return weapon;
        }
    }
}