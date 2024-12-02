
using System;
using Authoring;
using NonECS.ScriptableObjects;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShipECS.Entities
{
    public struct ShipUpgradeLevels : IBufferElementData
    {
        public UpgradeType type;
        public int level;
    }
    
    public readonly partial struct UpgradeAspects : IAspect
    {
        private readonly RefRW<HealthComponent> _health;
        private readonly RefRW<ProjectileAttack> _projectile;
        private readonly RefRW<PlayerBonusStat> _bonusStats;
        private readonly RefRW<PickupRadiusComponent> _pickupRadiusComponent;
        private readonly RefRW<ExperienceContainer> _expContainer;
        private readonly DynamicBuffer<ShipUpgradeLevels> _upgradeLevels;

        public int GetUpgradeLevel(UpgradeType type)
        {
            foreach (var upgrade in _upgradeLevels)
            {
                if (upgrade.type == type)
                {
                    return upgrade.level;
                }
            }
            return 1;
        }

        public void UpgradeShip(UpgradeType type)
        {
            var selectedIndex = -1;
            for (var i = 0; i < _upgradeLevels.Length; i++)
            {
                if (_upgradeLevels[i].type != type) continue;
                selectedIndex = i;
                break;
            }

            if (selectedIndex <= 0) return;
            
            var upgradeLevel = _upgradeLevels[selectedIndex].level;
            upgradeLevel++;

            _upgradeLevels.RemoveAt(selectedIndex);
            _upgradeLevels.Add(new ShipUpgradeLevels()
            {
                level = upgradeLevel,
                type = type
            });
        }

        public void ApplyUpgrades(UpgradeType type, float amount)
        {
            UpgradeShip(type);
            Debug.Log("Upgrading Success");
            
            switch (type)
            {
                case UpgradeType.LifetimeBonus:
                    Lifetime += amount;
                    break;
                case UpgradeType.NumCountBonus:
                    Count += Mathf.CeilToInt(amount);
                    break;
                case UpgradeType.DamageBonus:
                    Damage += amount;
                    break;
                case UpgradeType.SpeedBonus:
                    Speed += amount;
                    break;
                case UpgradeType.SizeBonus:
                    Size += amount;
                    break;
                case UpgradeType.FireRateReductionBonus:
                    FireRate += amount;
                    break;
                case UpgradeType.KnockbackBonus:
                    KnockBack += amount;
                    break;
                case UpgradeType.RangeBonus:
                    Range += amount;
                    break;
                case UpgradeType.ExpBonus:
                    Exp += amount;
                    break;
                case UpgradeType.RadiusBonus:
                    Radius += amount;
                    break;
                case UpgradeType.MaxHealth:
                    MaxHealth += amount;
                    break;
                case UpgradeType.Projectile:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public float MaxHealth
        {
            get => _health.ValueRO.MaxHealth;
            set => _health.ValueRW.MaxHealth = value;
        } 
        public float Radius
        {
            get => _pickupRadiusComponent.ValueRO.PickupRadiusBonus;
            set => _pickupRadiusComponent.ValueRW.PickupRadiusBonus = value;
        }
        
        public float Speed
        {
            get => _bonusStats.ValueRO.SpeedBonus;
            set => _bonusStats.ValueRW.SpeedBonus = value;
        }

        public float Exp
        {
            get => _expContainer.ValueRO.BonusExperience;
            set => _expContainer.ValueRW.BonusExperience = value;
        }

        public float KnockBack
        {
            get => _bonusStats.ValueRO.KnockbackBonus;
            set => _bonusStats.ValueRW.KnockbackBonus = value;
        }

        public float Lifetime
        {
            get => _bonusStats.ValueRO.LifetimeBonus;
            set => _bonusStats.ValueRW.LifetimeBonus = value;
        }

        public float Damage
        {
            get => _bonusStats.ValueRO.DamageBonus;
            set => _bonusStats.ValueRW.DamageBonus = value;
        }
        
        public float FireRate
        {
            get => _bonusStats.ValueRO.FireRateReductionBonus;
            set => _bonusStats.ValueRW.FireRateReductionBonus = value;
        }
        
        public float Size
        {
            get => _bonusStats.ValueRO.SizeBonus;
            set => _bonusStats.ValueRW.SizeBonus = value;
        }

        public float Range
        {
            get => _bonusStats.ValueRO.RangeBonus;
            set => _bonusStats.ValueRW.RangeBonus = value;
        }

        public int Count
        {
            get => _bonusStats.ValueRO.NumCountBonus;
            set => _bonusStats.ValueRW.NumCountBonus = value;
        }
    }
}
