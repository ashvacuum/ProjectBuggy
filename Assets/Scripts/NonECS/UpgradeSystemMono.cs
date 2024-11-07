using UnityEngine;

namespace NonECS
{
    public enum UpgradeClass
    {
        Self = 1 << 0,
        Projectile = 1 << 1
    }
    
    public enum UpgradeContent
    {
        Bonus,
        Base
    }

    public enum BonusGlobalStat
    {
        Health,
        Duration,
        Count,
        Penetration,
        Damage,
        Speed,
        FireRate,
        Size,
        PickupRadius,
        Exp,
        Knockback,
        Range
    }
    public class UpgradeSystemMono : MonoBehaviour
    {
        
    }
    
    /*
     *
     *public float LifetimeBonus;
        public int NumCountBonus; 
        public int PenetrationBonus;
        public float DamageBonus;
        public float SpeedBonus;
        public float AttackTimeReductionBonus;
        public float SizeBonus;
        public float FireRateReductionBonus;
        public float KnockbackBonus;
        public float RangeBonus;
        public float ExpBonus;
        public float RadiusBonus;
     */
}
