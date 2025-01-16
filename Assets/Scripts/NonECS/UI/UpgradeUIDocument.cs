using System;
using System.Collections.Generic;
using Authoring;
using NonECS.BaseWeapons;
using NonECS.ScriptableObjects;
using ShipECS.Entities;
using ShipECS.Systems;
using ShipECS.Systems.Artillery;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class UpgradeUIDocument : MonoBehaviour
    {
        [SerializeField] private ProjectileWeaponBase _projectileUpgradeData;
        [SerializeField] private ArtilleryWeaponBase _artilleryUpgradeData;
        private UIDocument _document;
        private VisualElement _container;
        private EntityManager _entityManager;
        private Entity _targetEntity;
        [FormerlySerializedAs("upgradeOptions")] [SerializeField] private UpgradeOptions _upgradeOptions;
        
        private EntityQuery _levelUpQuery;
        private EntityQuery _upgradesQuery;
        private EntityQuery _timeManagerQuery;
        private int _currentLevelUpBuffers;
        private bool _isShowingBuffers;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Start()
        {
            _currentLevelUpBuffers = 0;
            _levelUpQuery = _entityManager.CreateEntityQuery(typeof(LevelUpBuffer));
            _timeManagerQuery = _entityManager.CreateEntityQuery(typeof(TimeManagerComponent));
            _upgradesQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<HealthComponent>(),
                ComponentType.ReadWrite<PlayerBonusStat>(),
                ComponentType.ReadWrite<PickupRadiusComponent>(),
                ComponentType.ReadWrite<ExperienceContainer>(),
                ComponentType.ReadWrite<ShipUpgradeLevels>()
            );
        }

        private void OnEnable()
        {
            var root = _document.rootVisualElement;
            _container = root.Q<VisualElement>("upgrade-container");
        }

        private void FixedUpdate()
        {
            if (!_levelUpQuery.TryGetSingletonBuffer<LevelUpBuffer>(out var levelUpBuffer)) return;
            if (levelUpBuffer.IsEmpty)
            {
                //Debug.Log("Level up buffers are empty");
                return;
            }
            
            _currentLevelUpBuffers = levelUpBuffer.Length;
            levelUpBuffer.Clear();

            _targetEntity = _upgradesQuery.GetSingletonEntity();
                
            if (_timeManagerQuery.TryGetSingletonRW(out RefRW<TimeManagerComponent> manager))
            {
                var isPaused = _currentLevelUpBuffers > 0;
                manager.ValueRW.IsPaused = _currentLevelUpBuffers > 0;
                Debug.Log($"Is paused? {isPaused}");
            }
            
            if (_currentLevelUpBuffers > 0 && !_isShowingBuffers)
            {
                //roll using luck stat to increase this to 4
                ShowUpgradeOptions(3);
            }
        }

        private void ShowUpgradeOptions(int numberRolls)
        {
            _isShowingBuffers = true;
            _currentLevelUpBuffers--;

            _container.Clear();

            if (!_upgradesQuery.IsEmpty)
            {
                if (_upgradesQuery.TryGetSingletonBuffer<ShipUpgradeLevels>(out var shipUpgradeLevels))
                {

                    var currentShipUpgradeLevels = new List<Tuple<UpgradeType, int>>(); 
                    foreach (var upgradeLevel in shipUpgradeLevels)
                    {
                        currentShipUpgradeLevels.Add(new Tuple<UpgradeType, int>(upgradeLevel.type, upgradeLevel.level));
                    }
                    
                    var upgrades = _upgradeOptions.GetRandomUpgradeType(numberRolls, currentShipUpgradeLevels);
                    
                    foreach (var upgrade in upgrades)
                    {
                        var button = CreateUpgradeButton(upgrade);
                        _container.Add(button);
                    }
                }
            }

            _container.style.display = DisplayStyle.Flex;
        }

        private Button CreateUpgradeButton(UpgradeSelection selection)
        {
            var button = new Button();
            button.AddToClassList("upgrade-button");

            var layout = new VisualElement();
            layout.AddToClassList("upgrade-layout");

            var title = new Label(selection.UpgradeType.ToString());
            title.AddToClassList("upgrade-title");

            var level = new Label($"Level: {selection.UpgradeLevel}");
            level.AddToClassList("upgrade-level");

            layout.Add(title);
            layout.Add(level);
            button.Add(layout);

            button.clicked += () =>
            {
                if (selection.UpgradeType == UpgradeType.Projectile)
                {
                    ModifyEntityStats(selection.UpgradeType, selection.UpgradeValue, selection.UpgradeLevel);
                }
                else
                {
                    ModifyEntityStats(selection.UpgradeType, selection.UpgradeValue);
                }

                HideUpgradeContainer();
            };

            return button;
        }
        
        private void HideUpgradeContainer()
        {
            _isShowingBuffers = false;
            _container.style.display = DisplayStyle.None;
        }

        private void ModifyEntityStats(UpgradeType upgradeType, float value, int level = -1)
        {
            var entities = _upgradesQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var currentEntity in entities)
            {
                var buffers = _entityManager.GetBuffer<ShipUpgradeLevels>(currentEntity);

                var removalIndex = -1;
                var previousLevel = -1;
                for (var i = 0; i < buffers.Length; i++)
                {
                    if (buffers[i].type != upgradeType) continue;
                    
                    removalIndex = i;
                    previousLevel = buffers[i].level;
                    break;
                }

                if (removalIndex >= 0)
                {
                    buffers.RemoveAt(removalIndex);
                    buffers.Add(new ShipUpgradeLevels
                    {
                        type = upgradeType,
                        level = previousLevel + 1
                    });
                }
                
                switch (upgradeType)
                {
                    case UpgradeType.Projectile:
                        if (_entityManager.HasComponent<ProjectileAttack>(currentEntity))
                        {
                            var actualIndex = level - 1;
                            UpgradeProjectile(actualIndex, currentEntity);
                        }
                        else
                        {
                            _entityManager.AddComponent<ProjectileAttack>(currentEntity);
                            
                           
                            
                            var actualIndex = 0; // refers to level 1
                            UpgradeProjectile(actualIndex, currentEntity);
                        }
                        
                        break;
                    case UpgradeType.Artillery:
                        if (_entityManager.HasComponent<ArtilleryAttack>(currentEntity))
                        {
                            var actualIndex = level - 1;
                            UpgradeProjectile(actualIndex, currentEntity);
                        }
                        else
                        {
                            _entityManager.AddComponent<ArtilleryAttack>(currentEntity);

                            if (!_entityManager.HasBuffer<ArtilleryTarget>(currentEntity))
                                _entityManager.AddBuffer<ArtilleryTarget>(currentEntity);

                            var actualIndex = 0;
                            UpgradeArtillery(actualIndex, currentEntity);
                        }

                        break;

                    case UpgradeType.MaxHealth:
                        if (_entityManager.HasComponent<HealthComponent>(currentEntity))
                        {
                            var healthComponent = _entityManager.GetComponentData<HealthComponent>(currentEntity);
                            healthComponent.MaxHealth = value;
                            _entityManager.SetComponentData(currentEntity, healthComponent);
                        }

                        break;

                    case UpgradeType.RadiusBonus:
                        if (_entityManager.HasComponent<PickupRadiusComponent>(currentEntity))
                        {
                            var radiusComponent = _entityManager.GetComponentData<PickupRadiusComponent>(currentEntity);
                            radiusComponent.PickupRadiusBonus = value;
                            _entityManager.SetComponentData(currentEntity, radiusComponent);
                        }

                        break;

                    case UpgradeType.SpeedBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.SpeedBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.ExpBonus:
                        if (_entityManager.HasComponent<ExperienceContainer>(currentEntity))
                        {
                            var expContainer = _entityManager.GetComponentData<ExperienceContainer>(currentEntity);
                            expContainer.BonusExperience = value;
                            _entityManager.SetComponentData(currentEntity, expContainer);
                        }

                        break;

                    case UpgradeType.KnockbackBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.KnockbackBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.LifetimeBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.LifetimeBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.DamageBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.DamageBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.FireRateReductionBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.FireRateReductionBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.SizeBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.SizeBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.RangeBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.RangeBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;

                    case UpgradeType.NumCountBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.NumCountBonus = (int)value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;
                    case UpgradeType.PenetrationBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.PenetrationBonus = (int)value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;
                    
                    case UpgradeType.CriticalBonus:
                        if (_entityManager.HasComponent<PlayerBonusStat>(currentEntity))
                        {
                            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(currentEntity);
                            bonusStats.CriticalBonus = value;
                            _entityManager.SetComponentData(currentEntity, bonusStats);
                        }

                        break;
                    default:
                        
                        
                        throw new ArgumentException($"Unsupported upgrade type: {upgradeType}");
                }
            }

            entities.Dispose();
        }

        private void UpgradeProjectile(int actualIndex, Entity currentEntity)
        {
            _entityManager.HasComponent<ProjectileAttack>(currentEntity);
            var projectileAttack = _entityManager.GetComponentData<ProjectileAttack>(currentEntity);
            
            if (actualIndex <= -1) return;
            projectileAttack.BaseDamage = _projectileUpgradeData.upgradeData[actualIndex].Damage != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Damage
                : projectileAttack.BaseDamage;
            projectileAttack.BaseKnockback = _projectileUpgradeData.upgradeData[actualIndex].Knockback != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Knockback
                : projectileAttack.BaseKnockback;
            projectileAttack.BasePenetration = _projectileUpgradeData.upgradeData[actualIndex].Penetration != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Penetration
                : projectileAttack.BasePenetration;
            projectileAttack.BaseRange = _projectileUpgradeData.upgradeData[actualIndex].Range != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Range
                : projectileAttack.BaseRange;
            projectileAttack.BaseSpeed = _projectileUpgradeData.upgradeData[actualIndex].Speed != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Speed
                : projectileAttack.BaseSpeed;
            projectileAttack.BaseSize = _projectileUpgradeData.upgradeData[actualIndex].WeaponSize != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].WeaponSize
                : projectileAttack.BaseSize;
            projectileAttack.BaseFireRate = _projectileUpgradeData.upgradeData[actualIndex].FireRate != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].FireRate
                : projectileAttack.BaseFireRate;

            projectileAttack.BaseLifeTime = _projectileUpgradeData.upgradeData[actualIndex].Lifetime != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Lifetime
                : projectileAttack.BaseLifeTime;

            projectileAttack.BaseNumProjectile = _projectileUpgradeData.upgradeData[actualIndex].Count != 0
                ? _projectileUpgradeData.upgradeData[actualIndex].Count
                : projectileAttack.BaseNumProjectile;

            _entityManager.SetComponentData(currentEntity, projectileAttack);
        }
        
        private void UpgradeArtillery(int actualIndex, Entity currentEntity)
        {
            _entityManager.HasComponent<ArtilleryAttack>(currentEntity);
            var artilleryAttack = _entityManager.GetComponentData<ArtilleryAttack>(currentEntity);
            
            if (actualIndex <= -1) return;
            artilleryAttack.BaseDamage = _artilleryUpgradeData.upgradeData[actualIndex].Damage != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Damage
                : artilleryAttack.BaseDamage;
            artilleryAttack.BaseKnockback = _artilleryUpgradeData.upgradeData[actualIndex].Knockback != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Knockback
                : artilleryAttack.BaseKnockback;
            artilleryAttack.BasePenetration = _artilleryUpgradeData.upgradeData[actualIndex].Penetration != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Penetration
                : artilleryAttack.BasePenetration;
            artilleryAttack.BaseRange = _artilleryUpgradeData.upgradeData[actualIndex].Range != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Range
                : artilleryAttack.BaseRange;
            artilleryAttack.BaseSpeed = _artilleryUpgradeData.upgradeData[actualIndex].Speed != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Speed
                : artilleryAttack.BaseSpeed;
            artilleryAttack.BaseSize = _artilleryUpgradeData.upgradeData[actualIndex].WeaponSize != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].WeaponSize
                : artilleryAttack.BaseSize;
            artilleryAttack.BaseFireRate = _artilleryUpgradeData.upgradeData[actualIndex].FireRate != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].FireRate
                : artilleryAttack.BaseFireRate;

            artilleryAttack.BaseLifeTime = _artilleryUpgradeData.upgradeData[actualIndex].Lifetime != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Lifetime
                : artilleryAttack.BaseLifeTime;

            artilleryAttack.BaseNumProjectile = _artilleryUpgradeData.upgradeData[actualIndex].Count != 0
                ? _artilleryUpgradeData.upgradeData[actualIndex].Count
                : artilleryAttack.BaseNumProjectile;

            _entityManager.SetComponentData(currentEntity, artilleryAttack);
        }
    }
}