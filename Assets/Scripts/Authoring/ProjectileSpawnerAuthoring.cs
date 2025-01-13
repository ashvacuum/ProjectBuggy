using System.Collections.Generic;
using Authoring.Projectiles;
using NonECS.BaseWeapons;
using ShipECS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class ProjectileSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private NewProjectileAuthoring[] projectile;

        class Baker : Baker<ProjectileSpawnerAuthoring>
        {
            public override void Bake(ProjectileSpawnerAuthoring authoring)
            {
                var spawnerEntity = GetEntity(TransformUsageFlags.None);
                var weapons = AddBuffer<ProjectileSpawnerComponent>(spawnerEntity);
                foreach (var projectile in authoring.projectile)
                {
                    weapons.Add(new ProjectileSpawnerComponent()
                    {
                        ProjectileToSpawn = GetEntity(projectile.gameObject, TransformUsageFlags.Dynamic),
                        Class = projectile.Class
                    });
                }
                
            }
        }


    }
    
    public struct ProjectileSpawnerComponent : IBufferElementData
    {
        public Entity ProjectileToSpawn;
        public WeaponClass Class;
    }


}
