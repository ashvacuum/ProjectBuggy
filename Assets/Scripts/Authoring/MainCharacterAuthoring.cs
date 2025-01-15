using System;
using NonECS.BaseWeapons;
using NonECS.ScriptableObjects;
using NonECS.UI;
using ShipECS.Entities;
using ShipECS.Systems;
using ShipECS.Systems.Artillery;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class MainCharacterAuthoring : MonoBehaviour
    {
        public float speed = 100f;
        public float rotSpeed = 20f;
        public float3 CameraOffset;
        public float CameraPitchOverride;
        public float CameraSpeedOverride;
        [FormerlySerializedAs("Health")] public float MaxHealth = 400f;
        public float NextTimeCanTakeDamage = 0.4f;
        public float InitialPickupradius = 300f;

        [Space(30)]
        //Player Bonus stats for upgrades
        public float LifetimeBonus;
        public int NumCountBonus; 
        public int PenetrationBonus;
        public float DamageBonus;
        public float SpeedBonus;
        public float SizeBonus;
        public float FireRateReductionBonus;
        public float KnockbackBonus;
        public float RangeBonus;
        public float ExpBonus;
        public float RadiusBonus;
        public float CriticalBonus;
        [Space(10)]
        public WeaponBase ProjectileStats; 
    
        
        public class Baker : Baker<MainCharacterAuthoring>
        {
            public override void Bake(MainCharacterAuthoring authoring)
            {
                var projectileLevel = 0;
                var artilleryLevel = 0;
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraFollow()
                {
                    Offset = authoring.CameraOffset,
                    CameraPitch = authoring.CameraPitchOverride,
                    CameraSpeed = authoring.CameraSpeedOverride
                });
                AddComponent(entity, new HealthComponent()
                {
                    MaxHealth = authoring.MaxHealth,
                    NextTimeToTakeDamage = authoring.NextTimeCanTakeDamage,
                    CurrentNextTimeToTakeDamage = 0,
                    CurrentHealth = authoring.MaxHealth,
                    PreviousHealth = authoring.MaxHealth,
                    WasDamagedCritical = false
                });
                AddComponent(entity, new PickupRadiusComponent()
                {
                    BasePickupRadius = authoring.InitialPickupradius,
                    PickupRadiusBonus = authoring.RadiusBonus
                });

                AddComponent(entity, new PlayerBonusStat()
                {
                    LifetimeBonus = authoring.LifetimeBonus,
                    NumCountBonus = authoring.NumCountBonus,
                    PenetrationBonus = authoring.PenetrationBonus,
                    DamageBonus = authoring.DamageBonus,
                    SpeedBonus = authoring.SpeedBonus,
                    SizeBonus = authoring.SizeBonus,
                    KnockbackBonus = authoring.KnockbackBonus,
                    RangeBonus = authoring.RangeBonus,
                    FireRateReductionBonus = authoring.FireRateReductionBonus,
                    CriticalBonus = authoring.CriticalBonus
                });
                var stats = authoring.ProjectileStats;

                if (authoring.ProjectileStats is ProjectileWeaponBase)
                {
                    projectileLevel = 1;
                    AddComponent(entity, new ProjectileAttack()
                    {
                        BaseFireRate = stats.upgradeData[0].FireRate,
                        BasePenetration = stats.upgradeData[0].Penetration,
                        BaseSize = stats.upgradeData[0].WeaponSize,
                        BaseNumProjectile = stats.upgradeData[0].Count,
                        BaseDamage = stats.upgradeData[0].Damage,
                        BaseLifeTime = stats.upgradeData[0].Lifetime,
                        BaseSpeed = stats.upgradeData[0].Speed,
                        CurrentFireRate = 0,
                        BaseKnockback = stats.upgradeData[0].Knockback,
                        BaseRange = stats.upgradeData[0].Range,
                        BaseCritical = stats.upgradeData[0].Critical
                    });
                } else if (authoring.ProjectileStats is ArtilleryWeaponBase)
                {
                    artilleryLevel = 1;
                    AddComponent(entity, new ArtilleryAttack()
                    {
                        BaseFireRate = stats.upgradeData[0].FireRate,
                        BasePenetration = stats.upgradeData[0].Penetration,
                        BaseSize = stats.upgradeData[0].WeaponSize,
                        BaseNumProjectile = stats.upgradeData[0].Count,
                        BaseDamage = stats.upgradeData[0].Damage,
                        BaseLifeTime = stats.upgradeData[0].Lifetime,
                        BaseSpeed = stats.upgradeData[0].Speed,
                        CurrentFireRate = 0,
                        BaseKnockback = stats.upgradeData[0].Knockback,
                        BaseRange = stats.upgradeData[0].Range,
                        BaseCritical = stats.upgradeData[0].Critical
                    });
                }

                AddComponent(entity, new CharacterData
                {
                    moveSpeed = authoring.speed,
                    rotSpeed = authoring.rotSpeed
                });
                AddComponent(entity, new InputsData() );
                AddComponent<PlayerTag>(entity);
                AddComponent<StatefulTriggerEventExclude>(entity);
                AddBuffer<StatefulCollisionEvent>(entity);
                AddBuffer<ExperienceBuffer>(entity);
                AddBuffer<LevelUpBuffer>(entity);
                AddBuffer<DamageNumberRequest>(entity);
                AddComponent(entity, new ExperienceContainer()
                {
                    TotalExperience = 0,
                    BonusExperience = authoring.ExpBonus
                });
                var upgradeBuffer = AddBuffer<ShipUpgradeLevels>(entity);
                foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
                {
                    var currentLevel = 0;
                    if (type == UpgradeType.Artillery)
                    {
                        currentLevel = projectileLevel;
                    }
                    else if(type == UpgradeType.Projectile)
                    {
                        currentLevel = artilleryLevel;
                    }
                    upgradeBuffer.Add(new ShipUpgradeLevels()
                    {
                        level = currentLevel,
                        type = type
                    });
                }
                
            }
        }
    }

    public struct PlayerTag : IComponentData { }

    public struct HealthComponent : IComponentData
    {
        public float CurrentHealth;
        public float MaxHealth;
        public float PreviousHealth;
        public float NextTimeToTakeDamage;
        public float CurrentNextTimeToTakeDamage;
        public bool WasDamagedCritical;

        public float HealthPercent => CurrentHealth / MaxHealth * 100;
    }

    public struct PickupRadiusComponent : IComponentData
    {
        public float BasePickupRadius;
        public float PickupRadiusBonus; //add to this value over time but starts at 0;
        public float TotalPickupRadius => BasePickupRadius + (PickupRadiusBonus/100 * PickupRadiusBonus);
    }
    

    public struct DamageComponent : IComponentData
    {
        public float Damage;
        public float CriticalChance;
    }
    
    /// <summary>
    /// Refers to the bonus stats the player has gained via upgrades outside of menu or via ship components
    /// </summary>
    public struct PlayerBonusStat : IComponentData
    {
        public float LifetimeBonus;
        public int NumCountBonus; 
        public int PenetrationBonus;
        public float DamageBonus;
        public float SpeedBonus;
        public float SizeBonus;
        public float FireRateReductionBonus;
        public float RangeBonus;
        public float KnockbackBonus;
        public float CriticalBonus;
    }
}
