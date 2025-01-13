using NonECS.BaseWeapons;
using ShipECS.Systems;
using Unity.Entities;
using Unity.Physics.Stateful;
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
                switch (authoring.Class)
                {
                    case WeaponClass.Projectile:
                        
                        AddComponent<ProjectileTag>(entity);
                        AddComponent<ProjectileMotion>(entity);
                        AddBuffer<StatefulTriggerEvent>(entity);
                        break;
                    case WeaponClass.Artillery:
                        //add motion and artillery tag
                        break;
                };
                AddComponent<NewSpawnRenderInvisibleTag>(entity);
                
            }
        }
    }

    public struct NewSpawnRenderInvisibleTag : IComponentData
    {
    };
}
