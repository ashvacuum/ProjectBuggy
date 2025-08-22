using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    // Extended behavior system for complex weapon mechanics
    [Serializable]
    public class WeaponBehaviorDefinition
    {
        [Header("Behavior Type")]
        public WeaponBehaviorType behaviorType;
        
        [Header("Behavior Parameters")]
        public List<BehaviorParameter> parameters = new List<BehaviorParameter>();
        
        [Header("Required Components")]
        public List<string> requiredComponents = new List<string>();
        
        [Header("Custom System")]
        public string customSystemName; // Optional: name of custom system to use
        
        public T GetParameter<T>(string paramName, T defaultValue = default(T))
        {
            var param = parameters.Find(p => p.name == paramName);
            if (param == null) return defaultValue;
            
            try
            {
                return (T)Convert.ChangeType(param.value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        public void SetParameter(string paramName, object value)
        {
            var param = parameters.Find(p => p.name == paramName);
            if (param == null)
            {
                parameters.Add(new BehaviorParameter { name = paramName, value = value.ToString() });
            }
            else
            {
                param.value = value.ToString();
            }
        }
    }
    
    [Serializable]
    public struct BehaviorParameter
    {
        public string name;
        public string value;
        public string description;
    }
    
    public enum WeaponBehaviorType
    {
        // Standard behaviors (use existing systems)
        StandardProjectile,
        StandardArtillery,
        
        // Complex behaviors (generate additional components/data)
        ContinuousBeam,
        ChainLightning,
        HomingMissile,
        OrbitalStrike,
        DroneSwarm,
        ShieldGenerator,
        PulseWave,
        GravityWell,
        
        // Special behaviors (require custom systems)
        CustomBehavior
    }
    
    // Data structures for different weapon behaviors
    [Serializable]
    public class BeamWeaponData
    {
        public float beamWidth = 1f;
        public float beamLength = 10f;
        public float chargeTime = 0.5f;
        public float sustainTime = 2f;
        public float energyConsumption = 10f;
        public bool pierceEnemies = true;
        public AnimationCurve damageOverTime = AnimationCurve.Linear(0, 1, 1, 1);
        public GameObject beamEffect;
        public GameObject chargeEffect;
    }
    
    [Serializable]
    public class ChainLightningData
    {
        public int maxChains = 3;
        public float chainRange = 5f;
        public float damageReductionPerChain = 0.8f;
        public float chainDelay = 0.1f;
        public bool canChainToSameTarget = false;
        public GameObject chainEffect;
    }
    
    [Serializable]
    public class HomingMissileData
    {
        public float trackingSpeed = 90f; // degrees per second
        public float maxTrackingRange = 15f;
        public float accelerationTime = 1f;
        public bool retargetOnKill = true;
        public float explosionRadius = 3f;
        public GameObject trailEffect;
    }
    
    [Serializable]
    public class DroneSwarmData
    {
        public int maxDrones = 5;
        public float droneLifetime = 10f;
        public float droneSpeed = 8f;
        public float droneDamage = 5f;
        public float spawnDelay = 0.2f;
        public float followDistance = 3f;
        public GameObject dronePrefab;
    }
    
    // Factory for creating behavior-specific data
    public static class WeaponBehaviorFactory
    {
        public static List<string> GetRequiredComponents(WeaponBehaviorType behaviorType)
        {
            return behaviorType switch
            {
                WeaponBehaviorType.ContinuousBeam => new List<string> 
                { 
                    "BeamWeaponComponent", 
                    "EnergyComponent",
                    "ChargeComponent" 
                },
                WeaponBehaviorType.ChainLightning => new List<string> 
                { 
                    "ChainLightningComponent", 
                    "TargetChainBuffer" 
                },
                WeaponBehaviorType.HomingMissile => new List<string> 
                { 
                    "HomingComponent", 
                    "TargetEntity" 
                },
                WeaponBehaviorType.DroneSwarm => new List<string> 
                { 
                    "DroneSpawnerComponent", 
                    "DroneBuffer" 
                },
                WeaponBehaviorType.OrbitalStrike => new List<string> 
                { 
                    "OrbitalStrikeComponent", 
                    "DelayedActivation" 
                },
                _ => new List<string>()
            };
        }
        
        public static Dictionary<string, object> GetDefaultParameters(WeaponBehaviorType behaviorType)
        {
            return behaviorType switch
            {
                WeaponBehaviorType.ContinuousBeam => new Dictionary<string, object>
                {
                    ["beamWidth"] = 1f,
                    ["beamLength"] = 10f,
                    ["chargeTime"] = 0.5f,
                    ["sustainTime"] = 2f,
                    ["energyConsumption"] = 10f,
                    ["pierceEnemies"] = true
                },
                WeaponBehaviorType.ChainLightning => new Dictionary<string, object>
                {
                    ["maxChains"] = 3,
                    ["chainRange"] = 5f,
                    ["damageReductionPerChain"] = 0.8f,
                    ["chainDelay"] = 0.1f,
                    ["canChainToSameTarget"] = false
                },
                WeaponBehaviorType.HomingMissile => new Dictionary<string, object>
                {
                    ["trackingSpeed"] = 90f,
                    ["maxTrackingRange"] = 15f,
                    ["accelerationTime"] = 1f,
                    ["retargetOnKill"] = true,
                    ["explosionRadius"] = 3f
                },
                WeaponBehaviorType.DroneSwarm => new Dictionary<string, object>
                {
                    ["maxDrones"] = 5,
                    ["droneLifetime"] = 10f,
                    ["droneSpeed"] = 8f,
                    ["droneDamage"] = 5f,
                    ["spawnDelay"] = 0.2f,
                    ["followDistance"] = 3f
                },
                _ => new Dictionary<string, object>()
            };
        }
        
        public static string GetCustomSystemName(WeaponBehaviorType behaviorType)
        {
            return behaviorType switch
            {
                WeaponBehaviorType.ContinuousBeam => "BeamWeaponSystem",
                WeaponBehaviorType.ChainLightning => "ChainLightningSystem",
                WeaponBehaviorType.HomingMissile => "HomingMissileSystem",
                WeaponBehaviorType.DroneSwarm => "DroneSwarmSystem",
                WeaponBehaviorType.OrbitalStrike => "OrbitalStrikeSystem",
                _ => null
            };
        }
    }
}