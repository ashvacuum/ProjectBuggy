using System;
using System.Collections.Generic;
using Authoring;
using NonECS.ScriptableObjects;
using ShipECS.Entities;
using ShipECS.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace NonECS.UI
{
    public class UpgradeUIController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private UpgradeOptions upgradeOptions;
        [SerializeField] private UpgradeUIConfig uiConfig;
        
        private UIDocument _document;
        private VisualElement _container;
        private EntityManager _entityManager;
        private Entity _playerEntity;
        
        private EntityQuery _levelUpQuery;
        private EntityQuery _timeManagerQuery;
        private EntityQuery _upgradesQuery;
        
        private int _currentLevelUpBuffers;
        private bool _isShowingUpgrades;
        
        public void Initialize(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _document = GetComponent<UIDocument>();
            
            SetupQueries();
            SetupUI();
            
            if (UIManager.Instance != null)
                UIManager.Instance.OnPlayerEntityChanged += OnPlayerEntityChanged;
        }
        
        private void SetupQueries()
        {
            _levelUpQuery = _entityManager.CreateEntityQuery(typeof(LevelUpBuffer));
            _timeManagerQuery = _entityManager.CreateEntityQuery(typeof(TimeManagerComponent));
            _upgradesQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadWrite<HealthComponent>(),
                ComponentType.ReadWrite<WeaponBonusStat>(),
                ComponentType.ReadWrite<PickupRadiusComponent>(),
                ComponentType.ReadWrite<ExperienceContainer>(),
                ComponentType.ReadWrite<ShipUpgradeLevels>()
            );
        }
        
        private void SetupUI()
        {
            var root = _document.rootVisualElement;
            _container = root.Q<VisualElement>("upgrade-container");
            
            if (_container == null)
            {
                Debug.LogError("Upgrade container not found in UI document");
                return;
            }
            
            _container.style.display = DisplayStyle.None;
        }
        
        private void OnPlayerEntityChanged(Entity playerEntity)
        {
            _playerEntity = playerEntity;
        }
        
        private void FixedUpdate()
        {
            if (!_levelUpQuery.TryGetSingletonBuffer<LevelUpBuffer>(out var levelUpBuffer)) 
                return;
                
            if (levelUpBuffer.IsEmpty) 
                return;
            
            _currentLevelUpBuffers = levelUpBuffer.Length;
            levelUpBuffer.Clear();
            
            UpdatePauseState();
            
            if (_currentLevelUpBuffers > 0 && !_isShowingUpgrades)
            {
                ShowUpgradeOptions(uiConfig.NumberOfUpgradeChoices);
            }
        }
        
        private void UpdatePauseState()
        {
            if (_timeManagerQuery.TryGetSingletonRW(out RefRW<TimeManagerComponent> manager))
            {
                manager.ValueRW.IsPaused = _currentLevelUpBuffers > 0;
            }
        }
        
        private void ShowUpgradeOptions(int numberRolls)
        {
            _isShowingUpgrades = true;
            _currentLevelUpBuffers--;
            
            _container.Clear();
            
            var upgrades = GetAvailableUpgrades(numberRolls);
            
            foreach (var upgrade in upgrades)
            {
                var button = CreateUpgradeButton(upgrade);
                _container.Add(button);
            }
            
            _container.style.display = DisplayStyle.Flex;
        }
        
        private List<UpgradeSelection> GetAvailableUpgrades(int numberRolls)
        {
            if (!_upgradesQuery.TryGetSingletonBuffer<ShipUpgradeLevels>(out var shipUpgradeLevels))
                return new List<UpgradeSelection>();
            
            var currentLevels = new List<Tuple<UpgradeType, int>>();
            foreach (var upgradeLevel in shipUpgradeLevels)
            {
                currentLevels.Add(new Tuple<UpgradeType, int>(upgradeLevel.type, upgradeLevel.level));
            }
            
            return upgradeOptions.GetRandomUpgradeType(numberRolls, currentLevels);
        }
        
        private Button CreateUpgradeButton(UpgradeSelection selection)
        {
            var button = new Button();
            button.AddToClassList(uiConfig.UpgradeButtonClass);
            
            var layout = new VisualElement();
            layout.AddToClassList(uiConfig.UpgradeLayoutClass);
            
            var title = new Label(GetUpgradeDisplayName(selection.UpgradeType));
            title.AddToClassList(uiConfig.UpgradeTitleClass);
            
            var level = new Label($"Level: {selection.UpgradeLevel}");
            level.AddToClassList(uiConfig.UpgradeLevelClass);
            
            var description = new Label(GetUpgradeDescription(selection));
            description.AddToClassList(uiConfig.UpgradeDescriptionClass);
            
            layout.Add(title);
            layout.Add(level);
            layout.Add(description);
            button.Add(layout);
            
            button.clicked += () => OnUpgradeSelected(selection);
            
            return button;
        }
        
        private void OnUpgradeSelected(UpgradeSelection selection)
        {
            var upgradeSystem = new UpgradeApplicationSystem(_entityManager);
            upgradeSystem.ApplyUpgrade(_playerEntity, selection);
            
            HideUpgradeContainer();
        }
        
        private void HideUpgradeContainer()
        {
            _isShowingUpgrades = false;
            _container.style.display = DisplayStyle.None;
        }
        
        private string GetUpgradeDisplayName(UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                UpgradeType.Projectile => "Bullet System",
                UpgradeType.Artillery => "Artillery System", 
                UpgradeType.MaxHealth => "Health Boost",
                UpgradeType.SpeedBonus => "Speed Boost",
                UpgradeType.DamageBonus => "Damage Boost",
                UpgradeType.FireRateReductionBonus => "Fire Rate Boost",
                UpgradeType.ExpBonus => "Experience Boost",
                UpgradeType.RadiusBonus => "Pickup Range",
                _ => upgradeType.ToString()
            };
        }
        
        private string GetUpgradeDescription(UpgradeSelection selection)
        {
            return selection.UpgradeType switch
            {
                UpgradeType.Projectile => "Fires bullets at enemies",
                UpgradeType.Artillery => "Launches explosive shells",
                UpgradeType.MaxHealth => $"+{selection.UpgradeValue} Health",
                UpgradeType.SpeedBonus => $"+{selection.UpgradeValue}% Movement Speed",
                UpgradeType.DamageBonus => $"+{selection.UpgradeValue}% Damage",
                UpgradeType.FireRateReductionBonus => $"+{selection.UpgradeValue}% Fire Rate",
                UpgradeType.ExpBonus => $"+{selection.UpgradeValue}% Experience",
                UpgradeType.RadiusBonus => $"+{selection.UpgradeValue} Pickup Range",
                _ => "Improves ship capabilities"
            };
        }
        
        private void OnDestroy()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OnPlayerEntityChanged -= OnPlayerEntityChanged;
        }
    }
}