using UnityEngine;

namespace NonECS.BaseWeapons
{
    [CreateAssetMenu(menuName = "Weapon/Projectile", fileName = "Projectile")]
    public class ProjectileWeaponBase : WeaponBase
    {
        public GameObject ProjectilePrefab;
    }
}
