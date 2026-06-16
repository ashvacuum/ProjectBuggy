using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace NonECS.WeaponGeneration
{
    public static class WeaponCodeGenerator
    {
        private const string COMPONENTS_PATH = "Assets/Scripts/ShipECS/Components/Generated/";
        private const string WEAPON_BASE_PATH = "Assets/Scripts/NonECS/BaseWeapons/Generated/";
        private const string UPGRADE_TYPES_PATH = "Assets/Scripts/NonECS/ScriptableObjects/Generated/";
        
        public static void GenerateAllWeaponCode(List<WeaponDefinition> weaponDefinitions)
        {
            CreateDirectories();
            
            foreach (var weapon in weaponDefinitions)
            {
                GenerateWeaponAttackComponent(weapon);
                GenerateWeaponBase(weapon);
                GenerateUpgradeInfo(weapon);
            }
            
            GenerateUpgradeTypeEnum(weaponDefinitions);
            
            // Refresh Unity's asset database
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
            
            Debug.Log($"Generated code for {weaponDefinitions.Count} weapons!");
        }
        
        private static void CreateDirectories()
        {
            Directory.CreateDirectory(COMPONENTS_PATH);
            Directory.CreateDirectory(WEAPON_BASE_PATH);
            Directory.CreateDirectory(UPGRADE_TYPES_PATH);
        }
        
        public static void GenerateWeaponAttackComponent(WeaponDefinition weapon)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("using Unity.Entities;");
            sb.AppendLine("using Unity.Mathematics;");
            sb.AppendLine();
            sb.AppendLine("namespace ShipECS.Components.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    public struct {weapon.GetComponentName()} : IComponentData");
            sb.AppendLine("    {");
            
            // Generate all the standard weapon stats
            sb.AppendLine("        public float BaseFireRate;");
            sb.AppendLine("        public int BaseNumProjectile;");
            sb.AppendLine("        public int BasePenetration;");
            sb.AppendLine("        public float BaseDamage;");
            sb.AppendLine("        public float BaseRange;");
            sb.AppendLine("        public float BaseSize;");
            sb.AppendLine("        public float BaseLifeTime;");
            sb.AppendLine("        public float BaseSpeed;");
            sb.AppendLine("        public float BaseKnockback;");
            sb.AppendLine("        public float BaseCritical;");
            sb.AppendLine("        public float CurrentFireRate;");
            
            // Add special modifiers based on weapon behavior
            if (weapon.behaviorFlags.HasFlag(WeaponBehaviorFlags.RequiresChargeUp))
            {
                sb.AppendLine("        public float ChargeTime;");
                sb.AppendLine("        public float CurrentCharge;");
            }
            
            if (weapon.behaviorFlags.HasFlag(WeaponBehaviorFlags.ContinuousFire))
            {
                sb.AppendLine("        public bool IsFiring;");
                sb.AppendLine("        public float ContinuousDamageTimer;");
            }
            
            // Add behavior-specific fields based on complex behaviors
            foreach (var behavior in weapon.complexBehaviors)
            {
                AddBehaviorSpecificFields(sb, behavior);
            }
            
            sb.AppendLine("    }");
            
            // Generate additional components for complex behaviors
            foreach (var behavior in weapon.complexBehaviors)
            {
                GenerateAdditionalComponents(sb, behavior, weapon);
            }
            
            sb.AppendLine("}");
            
            var filePath = Path.Combine(COMPONENTS_PATH, $"{weapon.GetComponentName()}.cs");
            File.WriteAllText(filePath, sb.ToString());
        }
        
        private static void AddBehaviorSpecificFields(StringBuilder sb, WeaponBehaviorDefinition behavior)
        {
            switch (behavior.behaviorType)
            {
                case WeaponBehaviorType.ContinuousBeam:
                    sb.AppendLine("        // Beam weapon fields");
                    sb.AppendLine("        public float BeamWidth;");
                    sb.AppendLine("        public float BeamLength;");
                    sb.AppendLine("        public float EnergyConsumption;");
                    sb.AppendLine("        public float CurrentEnergy;");
                    sb.AppendLine("        public bool IsCharging;");
                    break;
                    
                case WeaponBehaviorType.ChainLightning:
                    sb.AppendLine("        // Chain lightning fields");
                    sb.AppendLine("        public int MaxChains;");
                    sb.AppendLine("        public float ChainRange;");
                    sb.AppendLine("        public float DamageReductionPerChain;");
                    sb.AppendLine("        public float ChainDelay;");
                    break;
                    
                case WeaponBehaviorType.HomingMissile:
                    sb.AppendLine("        // Homing missile fields");
                    sb.AppendLine("        public float TrackingSpeed;");
                    sb.AppendLine("        public float MaxTrackingRange;");
                    sb.AppendLine("        public float AccelerationTime;");
                    sb.AppendLine("        public bool RetargetOnKill;");
                    break;
                    
                case WeaponBehaviorType.DroneSwarm:
                    sb.AppendLine("        // Drone swarm fields");
                    sb.AppendLine("        public int MaxDrones;");
                    sb.AppendLine("        public float DroneLifetime;");
                    sb.AppendLine("        public float DroneSpeed;");
                    sb.AppendLine("        public float DroneDamage;");
                    sb.AppendLine("        public float SpawnDelay;");
                    break;
            }
        }
        
        private static void GenerateAdditionalComponents(StringBuilder sb, WeaponBehaviorDefinition behavior, WeaponDefinition weapon)
        {
            var weaponName = weapon.weaponName;
            
            switch (behavior.behaviorType)
            {
                case WeaponBehaviorType.ContinuousBeam:
                    sb.AppendLine();
                    sb.AppendLine($"    public struct {weaponName}BeamState : IComponentData");
                    sb.AppendLine("    {");
                    sb.AppendLine("        public float3 BeamStart;");
                    sb.AppendLine("        public float3 BeamEnd;");
                    sb.AppendLine("        public bool IsActive;");
                    sb.AppendLine("        public float ActiveTime;");
                    sb.AppendLine("    }");
                    break;
                    
                case WeaponBehaviorType.ChainLightning:
                    sb.AppendLine();
                    sb.AppendLine($"    public struct {weaponName}ChainTarget : IBufferElementData");
                    sb.AppendLine("    {");
                    sb.AppendLine("        public Entity Target;");
                    sb.AppendLine("        public float Damage;");
                    sb.AppendLine("        public int ChainLevel;");
                    sb.AppendLine("    }");
                    break;
                    
                case WeaponBehaviorType.HomingMissile:
                    sb.AppendLine();
                    sb.AppendLine($"    public struct {weaponName}HomingData : IComponentData");
                    sb.AppendLine("    {");
                    sb.AppendLine("        public Entity Target;");
                    sb.AppendLine("        public float AccelerationTimer;");
                    sb.AppendLine("        public bool HasTarget;");
                    sb.AppendLine("    }");
                    break;
                    
                case WeaponBehaviorType.DroneSwarm:
                    sb.AppendLine();
                    sb.AppendLine($"    public struct {weaponName}DroneData : IBufferElementData");
                    sb.AppendLine("    {");
                    sb.AppendLine("        public Entity DroneEntity;");
                    sb.AppendLine("        public float LifetimeRemaining;");
                    sb.AppendLine("        public float3 RelativePosition;");
                    sb.AppendLine("    }");
                    break;
            }
        }
        
        public static void GenerateWeaponBase(WeaponDefinition weapon)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using NonECS.BaseWeapons;");
            sb.AppendLine();
            sb.AppendLine("namespace NonECS.BaseWeapons.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    [CreateAssetMenu(menuName = \"Weapon/{weapon.displayName}\", fileName = \"{weapon.weaponName}\")]");
            sb.AppendLine($"    public class {weapon.GetWeaponBaseName()} : WeaponBase");
            sb.AppendLine("    {");
            
            // Add weapon-specific fields
            if (weapon.projectilePrefab != null)
            {
                sb.AppendLine("        public GameObject ProjectilePrefab;");
            }
            
            if (weapon.muzzleFlashPrefab != null)
            {
                sb.AppendLine("        public GameObject MuzzleFlashPrefab;");
            }
            
            if (weapon.impactEffectPrefab != null)
            {
                sb.AppendLine("        public GameObject ImpactEffectPrefab;");
            }
            
            if (weapon.fireSound != null)
            {
                sb.AppendLine("        public AudioClip FireSound;");
            }
            
            if (weapon.impactSound != null)
            {
                sb.AppendLine("        public AudioClip ImpactSound;");
            }
            
            // Add behavior flags
            sb.AppendLine($"        public WeaponBehaviorFlags BehaviorFlags = WeaponBehaviorFlags.{weapon.behaviorFlags};");
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            var filePath = Path.Combine(WEAPON_BASE_PATH, $"{weapon.GetWeaponBaseName()}.cs");
            File.WriteAllText(filePath, sb.ToString());
        }
        
        public static void GenerateUpgradeInfo(WeaponDefinition weapon)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using NonECS.BaseWeapons;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace NonECS.ScriptableObjects.Generated");
            sb.AppendLine("{");
            sb.AppendLine($"    [CreateAssetMenu(menuName = \"Weapon Data/{weapon.displayName} Data\", fileName = \"{weapon.weaponName}Data\")]");
            sb.AppendLine($"    public class {weapon.weaponName}Data : ScriptableObject");
            sb.AppendLine("    {");
            sb.AppendLine("        [Header(\"Base Stats\")]");
            sb.AppendLine("        public WeaponStats baseStats;");
            sb.AppendLine();
            sb.AppendLine("        [Header(\"Upgrade Levels\")]");
            sb.AppendLine("        public List<UpgradeInfo> upgradeLevels = new List<UpgradeInfo>();");
            sb.AppendLine();
            sb.AppendLine("        [Header(\"Description\")]");
            sb.AppendLine($"        [TextArea(3, 5)]");
            sb.AppendLine($"        public string description = \"{weapon.description}\";");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            var filePath = Path.Combine(UPGRADE_TYPES_PATH, $"{weapon.weaponName}Data.cs");
            File.WriteAllText(filePath, sb.ToString());
        }
        
        public static void GenerateUpgradeTypeEnum(List<WeaponDefinition> weaponDefinitions)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("// Auto-generated enum for weapon upgrade types");
            sb.AppendLine("namespace NonECS.ScriptableObjects.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    public enum GeneratedUpgradeType");
            sb.AppendLine("    {");
            
            // Add existing upgrade types first
            sb.AppendLine("        // Existing upgrade types");
            sb.AppendLine("        LifetimeBonus = 1,");
            sb.AppendLine("        NumCountBonus = 2,");
            sb.AppendLine("        DamageBonus = 3,");
            sb.AppendLine("        SpeedBonus = 4,");
            sb.AppendLine("        SizeBonus = 5,");
            sb.AppendLine("        FireRateReductionBonus = 6,");
            sb.AppendLine("        KnockbackBonus = 7,");
            sb.AppendLine("        RangeBonus = 8,");
            sb.AppendLine("        ExpBonus = 9,");
            sb.AppendLine("        RadiusBonus = 10,");
            sb.AppendLine("        MaxHealth = 11,");
            sb.AppendLine("        PenetrationBonus = 12,");
            sb.AppendLine("        CriticalBonus = 13,");
            sb.AppendLine();
            
            sb.AppendLine("        // Generated weapon types");
            var startIndex = 100; // Start generated weapons at 100 to avoid conflicts
            
            for (int i = 0; i < weaponDefinitions.Count; i++)
            {
                var weapon = weaponDefinitions[i];
                sb.AppendLine($"        {weapon.GetUpgradeTypeName()} = {startIndex + i},");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            var filePath = Path.Combine(UPGRADE_TYPES_PATH, "GeneratedUpgradeTypes.cs");
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}