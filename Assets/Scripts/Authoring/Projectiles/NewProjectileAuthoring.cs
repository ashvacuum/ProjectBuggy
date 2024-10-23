using NonECS.BaseWeapons;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring.Projectiles
{
    public class NewProjectileAuthoring : MonoBehaviour
    {
        public WeaponClass Class;
        private class NewProjectileBaker : Baker<NewProjectileAuthoring>
        {
            public override void Bake(NewProjectileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ProjectileTag>(entity);
                AddComponent<ProjectileMotion>(entity);
            }
        }
    }
}
