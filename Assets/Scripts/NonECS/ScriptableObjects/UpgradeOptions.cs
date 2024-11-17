using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        MaxHealth = 11
    }
    [Serializable]
    public struct UpgradeInfo
    {
        public UpgradeType UpgradeType;
        public float UpgradeChance;
        public List<float> UpgradeLevels;
    }
    [CreateAssetMenu(menuName = "Upgrade Options", fileName = "Upgrade Options")]
    public class UpgradeOptions : ScriptableObject
    {
        public List<UpgradeInfo> UpgradeInfos;
        public List<UpgradeInfo> GetRandomUpgradeType(int numberRolls)
        {
            var returnedUpgrades = new List<UpgradeInfo>();
            for (var i = 0; i < numberRolls; i++)
            {
                var min = 0f;
                var max = 0f;
                var totals = UpgradeInfos.Sum(upgrade => upgrade.UpgradeChance);

                var randomRoll = UnityEngine.Random.Range(min, totals);
                foreach (var upgrade in UpgradeInfos)
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
    }
}
