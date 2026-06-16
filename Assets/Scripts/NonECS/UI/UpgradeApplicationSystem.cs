using System;
using Authoring;
using NonECS.BaseWeapons;
using NonECS.ScriptableObjects;
using ShipECS.Entities;
using ShipECS.Systems;
using ShipECS.Systems.Artillery;
using Unity.Entities;
using UnityEngine;

namespace NonECS.UI
{
    public class UpgradeApplicationSystem
    {
        private readonly EntityManager _entityManager;
        
        public UpgradeApplicationSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public void ApplyUpgrade(Entity targetEntity, UpgradeSelection selection)
        {
            UpdateUpgradeLevel(targetEntity, selection.UpgradeType);
            
            switch (selection.UpgradeType)
            {
                case UpgradeType.Projectile:
                    ApplyProjectileUpgrade(targetEntity, selection);
                    break;
                case UpgradeType.Artillery:
                    ApplyArtilleryUpgrade(targetEntity, selection);
                    break;
                case UpgradeType.MaxHealth:
                    ApplyHealthUpgrade(targetEntity, selection.UpgradeValue);
                    break;
                case UpgradeType.SpeedBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue, 
                        (PlayerBonusStat stats, float value) => stats.SpeedBonus = value);
                    break;
                case UpgradeType.DamageBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.DamageBonus = value);
                    break;
                case UpgradeType.FireRateReductionBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.FireRateReductionBonus = value);
                    break;
                case UpgradeType.ExpBonus:
                    ApplyExperienceUpgrade(targetEntity, selection.UpgradeValue);
                    break;
                case UpgradeType.RadiusBonus:
                    ApplyRadiusUpgrade(targetEntity, selection.UpgradeValue);
                    break;
                case UpgradeType.KnockbackBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.KnockbackBonus = value);
                    break;
                case UpgradeType.LifetimeBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.LifetimeBonus = value);
                    break;
                case UpgradeType.SizeBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.SizeBonus = value);
                    break;
                case UpgradeType.RangeBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.RangeBonus = value);
                    break;
                case UpgradeType.NumCountBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.NumCountBonus = (int)value);
                    break;
                case UpgradeType.PenetrationBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.PenetrationBonus = (int)value);
                    break;
                case UpgradeType.CriticalBonus:
                    ApplyBonusStatUpgrade(targetEntity, selection.UpgradeValue,
                        (PlayerBonusStat stats, float value) => stats.CriticalBonus = value);
                    break;
                default:
                    Debug.LogWarning($"Upgrade type {selection.UpgradeType} not implemented");
                    break;
            }
        }
        
        private void UpdateUpgradeLevel(Entity entity, UpgradeType upgradeType)
        {
            var buffer = _entityManager.GetBuffer<ShipUpgradeLevels>(entity);
            
            var removalIndex = -1;
            var previousLevel = 0;
            
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].type != upgradeType) continue;
                removalIndex = i;
                previousLevel = buffer[i].level;
                break;
            }
            
            if (removalIndex >= 0)
            {
                buffer.RemoveAt(removalIndex);
            }
            
            buffer.Add(new ShipUpgradeLevels
            {
                type = upgradeType,
                level = previousLevel + 1
            });
        }
        
        private void ApplyProjectileUpgrade(Entity entity, UpgradeSelection selection)
        {
            if (!_entityManager.HasComponent<ProjectileAttack>(entity))
            {
                _entityManager.AddComponent<ProjectileAttack>(entity);
            }
            
            // Apply upgrade data from ScriptableObject
            // This would need the weapon upgrade data reference
            Debug.Log($"Applied Projectile upgrade level {selection.UpgradeLevel}");
        }
        
        private void ApplyArtilleryUpgrade(Entity entity, UpgradeSelection selection)
        {
            if (!_entityManager.HasComponent<ArtilleryAttack>(entity))
            {
                _entityManager.AddComponent<ArtilleryAttack>(entity);
                
                if (!_entityManager.HasBuffer<ArtilleryTarget>(entity))
                    _entityManager.AddBuffer<ArtilleryTarget>(entity);
            }
            
            Debug.Log($"Applied Artillery upgrade level {selection.UpgradeLevel}");
        }
        
        private void ApplyHealthUpgrade(Entity entity, float value)
        {
            if (!_entityManager.HasComponent<HealthComponent>(entity)) return;
            
            var health = _entityManager.GetComponentData<HealthComponent>(entity);
            health.MaxHealth = value;
            _entityManager.SetComponentData(entity, health);
        }
        
        private void ApplyBonusStatUpgrade(Entity entity, float value, Action<PlayerBonusStat, float> setter)
        {
            if (!_entityManager.HasComponent<PlayerBonusStat>(entity)) return;
            
            var bonusStats = _entityManager.GetComponentData<PlayerBonusStat>(entity);
            setter(bonusStats, value);
            _entityManager.SetComponentData(entity, bonusStats);
        }
        
        private void ApplyExperienceUpgrade(Entity entity, float value)
        {
            if (!_entityManager.HasComponent<ExperienceContainer>(entity)) return;
            
            var expContainer = _entityManager.GetComponentData<ExperienceContainer>(entity);
            expContainer.BonusExperience = value;
            _entityManager.SetComponentData(entity, expContainer);
        }
        
        private void ApplyRadiusUpgrade(Entity entity, float value)
        {
            if (!_entityManager.HasComponent<PickupRadiusComponent>(entity)) return;
            
            var radius = _entityManager.GetComponentData<PickupRadiusComponent>(entity);
            radius.PickupRadiusBonus = value;
            _entityManager.SetComponentData(entity, radius);
        }
    }
}