using System;
using System.Collections.Generic;
using System.Linq;
using NonECS.BaseWeapons;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace NonECS.ScriptableObjects
{
    //[Flags]
    public enum UpgradeType
    {
        Projectile = 0,
        LifetimeBonus = 1,
        NumCountBonus = 2,
        DamageBonus = 3,
        SpeedBonus = 4,
        SizeBonus = 5,
        FireRateReductionBonus = 6,
        KnockbackBonus = 7,
        RangeBonus = 8,
        ExpBonus = 9,
        RadiusBonus = 10,
        MaxHealth = 11,
        PenetrationBonus = 12,
        CriticalBonus = 13,
        Artillery = 14
    }
    [Serializable]
    public class UpgradeInfo
    {
        public UpgradeType UpgradeType;
        public float UpgradeChance;
        public List<float> UpgradeLevels;
    }
    [Serializable]
    public struct UpgradeSelection
    {
        public UpgradeType UpgradeType { get; }
        public int UpgradeLevel { get; }
        public float UpgradeValue { get; }

        public UpgradeSelection(UpgradeType type, int level, float value)
        {
            UpgradeType = type;
            UpgradeLevel = level;
            UpgradeValue = value;
        }
    }
    [CreateAssetMenu(menuName = "Upgrade Options", fileName = "Upgrade Options")]
    public class UpgradeOptions : ScriptableObject
    {
        [SerializeField] private ProjectileWeaponBase _projectileUpgradeData;
        
        [FormerlySerializedAs("UpgradeInfos")] public List<UpgradeInfo> _upgradeInfos;
        
        public List<UpgradeSelection> GetRandomUpgradeType(int numberRolls, List<Tuple<UpgradeType,int>> CurrentLevels, bool isUnique = true)
        {
            var validUpgradeSelection = new List<UpgradeSelection>();
            
            var validUpgrades = _upgradeInfos.Where(upgrade =>
            {
                var matchingListTuple = CurrentLevels
                    .FirstOrDefault(listTuple => listTuple.Item1 == upgrade.UpgradeType);
                
                var isProjectile = upgrade.UpgradeType == UpgradeType.Projectile;
                if (isProjectile)
                {
                    return matchingListTuple != null &&
                           matchingListTuple.Item2 < _projectileUpgradeData.upgradeData.Count;
                }

                return matchingListTuple != null &&
                       matchingListTuple.Item2 < upgrade.UpgradeLevels.Count;
            }).ToList();
            
            for (var i = 0; i < numberRolls; i++)
            {
                var min = 0f;
                var max = 0f;

                
                var totals = validUpgrades.Sum(upgrade => upgrade.UpgradeChance);

                var randomRoll = UnityEngine.Random.Range(min, totals);
                UpgradeSelection? actualItem = null;
                foreach (var upgrade in validUpgrades)
                {
                    max += upgrade.UpgradeChance;
                    if (randomRoll <= max)
                    {
                        //validUpgradeSelection.Add();

                        var matchingUpgrade = CurrentLevels.FirstOrDefault(item => item.Item1 == upgrade.UpgradeType);
                        if (matchingUpgrade == null) continue;
                        var itemIndex = matchingUpgrade.Item2;
                        itemIndex++;
                        itemIndex = matchingUpgrade.Item1 == UpgradeType.Projectile
                            ? Mathf.Min(itemIndex, _projectileUpgradeData.upgradeData.Count)
                            : Mathf.Min(itemIndex, upgrade.UpgradeLevels.Count);


                        try
                        {
                            if (matchingUpgrade.Item1 == UpgradeType.Projectile)
                            {
                                actualItem = new UpgradeSelection(matchingUpgrade.Item1, itemIndex, 0);
                            }
                            else
                            {
                                actualItem = new UpgradeSelection(matchingUpgrade.Item1, itemIndex,
                                    upgrade.UpgradeLevels[itemIndex]);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(
                                $"Error: Type: {matchingUpgrade.Item1.ToString()}, Level {itemIndex}, Count {upgrade.UpgradeLevels.Count}");
                        }

                        break;
                    }

                    min = max;
                    continue;

                }

                if (!actualItem.HasValue) continue;
                
                validUpgradeSelection.Add(actualItem.Value);
                
                var selectionToRemove = validUpgrades.FirstOrDefault(s => s.UpgradeType == actualItem.Value.UpgradeType);
                
                if(selectionToRemove != null && isUnique)
                    validUpgrades.Remove(selectionToRemove);

            }

            return validUpgradeSelection;
        }
    }
}
