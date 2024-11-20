using System;
using NonECS.BaseWeapons;
using NonECS.ScriptableObjects;
using NonECS.UI;
using ShipECS.Entities;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Transforms;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering;
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
        [Space(10)]
        public ProjectileWeaponBase ProjectileStats; 
    
        
        public class Baker : Baker<MainCharacterAuthoring>
        {
            public override void Bake(MainCharacterAuthoring authoring)
            {
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
                    PreviousHealth = authoring.MaxHealth
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
                    FireRateReductionBonus = authoring.FireRateReductionBonus
                });
                var stats = authoring.ProjectileStats;
                AddComponent(entity, new ProjectileAttack()
                {
                    BaseFireRate = stats.BaseFireRate,
                    BasePenetration = stats.BasePenetration,
                    BaseSize = stats.WeaponSize,
                    BaseNumProjectile = stats.BaseCount,
                    BaseDamage = stats.BaseDamage,
                    BaseLifeTime = stats.BaseLifetime,
                    BaseSpeed = stats.BaseSpeed,
                    CurrentFireRate = 0,
                    BaseKnockback = stats.BaseKnockback,
                    BaseRange = stats.BaseRange
                });
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
                    upgradeBuffer.Add(new ShipUpgradeLevels()
                    {
                        level = 1,
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
    }
}
