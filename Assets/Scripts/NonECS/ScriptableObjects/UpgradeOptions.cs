using System;
using System.Collections.Generic;
using System.Linq;
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
        PenetrationBonus = 12
    }
    [Serializable]
    public struct UpgradeInfo
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
        [FormerlySerializedAs("UpgradeInfos")] public List<UpgradeInfo> _upgradeInfos;
        public List<UpgradeInfo> GetRandomUpgradeType(int numberRolls)
        {
            var returnedUpgrades = new List<UpgradeInfo>();
            
            for (var i = 0; i < numberRolls; i++)
            {
                var min = 0f;
                var max = 0f;
                var totals = _upgradeInfos.Sum(upgrade => upgrade.UpgradeChance);

                var randomRoll = UnityEngine.Random.Range(min, totals);
                
                foreach (var upgrade in _upgradeInfos)
                {
                    max += upgrade.UpgradeChance;
                    if (randomRoll <= max)
                    {
                        returnedUpgrades.Add(upgrade);
                        break;
                    }
                    else
                    {
                        min = max;
                        continue;
                    }
                }
            }

            return returnedUpgrades;
        }
        
        public List<UpgradeSelection> GetRandomUpgradeType(int numberRolls, List<Tuple<UpgradeType,int>> CurrentLevels)
        {
            var validUpgradeSelection = new List<UpgradeSelection>();
            for (var i = 0; i < numberRolls; i++)
            {
                var min = 0f;
                var max = 0f;

                var validUpgrades = _upgradeInfos.Where(upgrade =>
                {
                    var matchingListTuple = CurrentLevels
                        .FirstOrDefault(listTuple => listTuple.Item1 == upgrade.UpgradeType);

                    return matchingListTuple != null &&
                           matchingListTuple.Item2 < upgrade.UpgradeLevels.Count;
                }).ToList();
                var totals = validUpgrades.Sum(upgrade => upgrade.UpgradeChance);
                
                

                var randomRoll = UnityEngine.Random.Range(min, totals);
                
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
                        itemIndex = Mathf.Min(itemIndex, upgrade.UpgradeLevels.Count - 1);
                        validUpgradeSelection.Add(new UpgradeSelection(matchingUpgrade.Item1, itemIndex, upgrade.UpgradeLevels[itemIndex]));
                        
                        break;
                    }
                    else
                    {
                        min = max;
                        continue;
                    }
                }
            }

            return validUpgradeSelection;
        }
    }
}
