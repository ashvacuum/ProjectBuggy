using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    [Serializable]
    public class WeaponJsonData
    {
        public string weaponName;
        public string displayName;
        public string description;
        public string category;
        public string archetype;
        public WeaponStatsJson baseStats;
        public List<WeaponUpgradeLevelJson> upgradeLevels;
        public List<string> behaviorFlags;
        public List<UpgradeRequirementJson> unlockRequirements;
    }
    
    [Serializable]
    public class WeaponStatsJson
    {
        public float baseDamage = 10f;
        public float baseCritical = 0f;
        public float baseKnockback = 1f;
        public float baseFireRate = 1f;
        public int baseNumProjectiles = 1;
        public float baseRange = 10f;
        public float baseSpeed = 5f;
        public float baseLifetime = 2f;
        public float baseSize = 1f;
        public int basePenetration = 1;
    }
    
    [Serializable]
    public class WeaponUpgradeLevelJson
    {
        public int level;
        public string description;
        public WeaponStatsJson statMultipliers;
        public WeaponStatsJson statAdditions;
        public List<string> specialEffects;
    }
    
    [Serializable]
    public class UpgradeRequirementJson
    {
        public string requiredUpgradeType;
        public int minimumLevel;
        public string description;
    }
    
    public static class WeaponJsonImporter
    {
        public static WeaponDefinition ImportFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"JSON file not found: {jsonPath}");
                return null;
            }
            
            string jsonContent = File.ReadAllText(jsonPath);
            var jsonData = JsonUtility.FromJson<WeaponJsonData>(jsonContent);
            
            return ConvertJsonToWeaponDefinition(jsonData);
        }
        
        public static List<WeaponDefinition> ImportAllFromDirectory(string directoryPath)
        {
            var weapons = new List<WeaponDefinition>();
            
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"Directory not found: {directoryPath}");
                return weapons;
            }
            
            var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
            
            foreach (var file in jsonFiles)
            {
                var weapon = ImportFromJson(file);
                if (weapon != null)
                {
                    weapons.Add(weapon);
                }
            }
            
            Debug.Log($"Imported {weapons.Count} weapons from {directoryPath}");
            return weapons;
        }
        
        public static void ExportToJson(WeaponDefinition weapon, string outputPath)
        {
            var jsonData = ConvertWeaponDefinitionToJson(weapon);
            string jsonContent = JsonUtility.ToJson(jsonData, true);
            
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllText(outputPath, jsonContent);
            
            Debug.Log($"Exported weapon {weapon.weaponName} to {outputPath}");
        }
        
        private static WeaponDefinition ConvertJsonToWeaponDefinition(WeaponJsonData jsonData)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            
            weapon.weaponName = jsonData.weaponName;
            weapon.displayName = jsonData.displayName;
            weapon.description = jsonData.description;
            
            // Parse enums
            if (Enum.TryParse<WeaponCategory>(jsonData.category, out var category))
                weapon.category = category;
            
            if (Enum.TryParse<WeaponArchetype>(jsonData.archetype, out var archetype))
                weapon.archetype = archetype;
            
            // Convert stats
            weapon.baseStats = ConvertJsonToWeaponStats(jsonData.baseStats);
            
            // Convert upgrade levels
            weapon.upgradeLevels = new List<WeaponUpgradeLevel>();
            if (jsonData.upgradeLevels != null)
            {
                foreach (var levelJson in jsonData.upgradeLevels)
                {
                    weapon.upgradeLevels.Add(ConvertJsonToUpgradeLevel(levelJson));
                }
            }
            
            // Parse behavior flags
            weapon.behaviorFlags = WeaponBehaviorFlags.None;
            if (jsonData.behaviorFlags != null)
            {
                foreach (var flagString in jsonData.behaviorFlags)
                {
                    if (Enum.TryParse<WeaponBehaviorFlags>(flagString, out var flag))
                    {
                        weapon.behaviorFlags |= flag;
                    }
                }
            }
            
            // Convert requirements
            weapon.unlockRequirements = new List<UpgradeRequirement>();
            if (jsonData.unlockRequirements != null)
            {
                foreach (var reqJson in jsonData.unlockRequirements)
                {
                    weapon.unlockRequirements.Add(new UpgradeRequirement
                    {
                        requiredUpgradeType = reqJson.requiredUpgradeType,
                        minimumLevel = reqJson.minimumLevel,
                        description = reqJson.description
                    });
                }
            }
            
            return weapon;
        }
        
        private static WeaponStats ConvertJsonToWeaponStats(WeaponStatsJson jsonStats)
        {
            if (jsonStats == null) return WeaponStats.Default;
            
            return new WeaponStats
            {
                baseDamage = jsonStats.baseDamage,
                baseCritical = jsonStats.baseCritical,
                baseKnockback = jsonStats.baseKnockback,
                baseFireRate = jsonStats.baseFireRate,
                baseNumProjectiles = jsonStats.baseNumProjectiles,
                baseRange = jsonStats.baseRange,
                baseSpeed = jsonStats.baseSpeed,
                baseLifetime = jsonStats.baseLifetime,
                baseSize = jsonStats.baseSize,
                basePenetration = jsonStats.basePenetration
            };
        }
        
        private static WeaponUpgradeLevel ConvertJsonToUpgradeLevel(WeaponUpgradeLevelJson levelJson)
        {
            return new WeaponUpgradeLevel
            {
                level = levelJson.level,
                description = levelJson.description,
                statMultipliers = ConvertJsonToWeaponStats(levelJson.statMultipliers),
                statAdditions = ConvertJsonToWeaponStats(levelJson.statAdditions),
                specialEffects = levelJson.specialEffects ?? new List<string>()
            };
        }
        
        private static WeaponJsonData ConvertWeaponDefinitionToJson(WeaponDefinition weapon)
        {
            var jsonData = new WeaponJsonData
            {
                weaponName = weapon.weaponName,
                displayName = weapon.displayName,
                description = weapon.description,
                category = weapon.category.ToString(),
                archetype = weapon.archetype.ToString(),
                baseStats = ConvertWeaponStatsToJson(weapon.baseStats),
                behaviorFlags = new List<string>()
            };
            
            // Convert behavior flags
            foreach (WeaponBehaviorFlags flag in Enum.GetValues(typeof(WeaponBehaviorFlags)))
            {
                if (weapon.behaviorFlags.HasFlag(flag) && flag != WeaponBehaviorFlags.None)
                {
                    jsonData.behaviorFlags.Add(flag.ToString());
                }
            }
            
            // Convert upgrade levels
            jsonData.upgradeLevels = new List<WeaponUpgradeLevelJson>();
            foreach (var level in weapon.upgradeLevels)
            {
                jsonData.upgradeLevels.Add(new WeaponUpgradeLevelJson
                {
                    level = level.level,
                    description = level.description,
                    statMultipliers = ConvertWeaponStatsToJson(level.statMultipliers),
                    statAdditions = ConvertWeaponStatsToJson(level.statAdditions),
                    specialEffects = level.specialEffects
                });
            }
            
            // Convert requirements
            jsonData.unlockRequirements = new List<UpgradeRequirementJson>();
            foreach (var req in weapon.unlockRequirements)
            {
                jsonData.unlockRequirements.Add(new UpgradeRequirementJson
                {
                    requiredUpgradeType = req.requiredUpgradeType,
                    minimumLevel = req.minimumLevel,
                    description = req.description
                });
            }
            
            return jsonData;
        }
        
        private static WeaponStatsJson ConvertWeaponStatsToJson(WeaponStats stats)
        {
            return new WeaponStatsJson
            {
                baseDamage = stats.baseDamage,
                baseCritical = stats.baseCritical,
                baseKnockback = stats.baseKnockback,
                baseFireRate = stats.baseFireRate,
                baseNumProjectiles = stats.baseNumProjectiles,
                baseRange = stats.baseRange,
                baseSpeed = stats.baseSpeed,
                baseLifetime = stats.baseLifetime,
                baseSize = stats.baseSize,
                basePenetration = stats.basePenetration
            };
        }
        
        public static void CreateExampleJsonFiles(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            
            // Create example projectile weapon
            var projectileJson = new WeaponJsonData
            {
                weaponName = "BasicProjectile",
                displayName = "Basic Projectile",
                description = "A simple projectile weapon that fires bullets at enemies",
                category = "Projectile",
                archetype = "SingleTarget",
                baseStats = new WeaponStatsJson
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
                },
                upgradeLevels = new List<WeaponUpgradeLevelJson>
                {
                    new WeaponUpgradeLevelJson
                    {
                        level = 1,
                        description = "Fires 2 bullets",
                        statAdditions = new WeaponStatsJson { baseNumProjectiles = 1 },
                        specialEffects = new List<string>()
                    }
                },
                behaviorFlags = new List<string> { "AutoTarget", "DestroyOnContact" },
                unlockRequirements = new List<UpgradeRequirementJson>()
            };
            
            string jsonContent = JsonUtility.ToJson(projectileJson, true);
            File.WriteAllText(Path.Combine(outputDirectory, "BasicProjectile.json"), jsonContent);
            
            Debug.Log($"Created example JSON files in {outputDirectory}");
        }
    }
}