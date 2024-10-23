using System.Collections.Generic;
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
        [SerializeField] private GameObject projectile;

        class Baker : Baker<ProjectileSpawnerAuthoring>
        {
            public override void Bake(ProjectileSpawnerAuthoring authoring)
            {
                var spawnerEntity = GetEntity(TransformUsageFlags.None);
                AddComponent(spawnerEntity, new ProjectileSpawnerComponent()
                {
                    ProjectileToSpawn = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic)
                });
            }
        }


    }
    
    public struct ProjectileSpawnerComponent : IComponentData
    {
        public Entity ProjectileToSpawn;
    }


}
