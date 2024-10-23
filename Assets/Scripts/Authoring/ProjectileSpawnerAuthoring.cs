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
       [SerializeField]private ProjectileWeaponBase projectile;

       class Baker : Baker<ProjectileSpawnerAuthoring>
       {
           public override void Bake(ProjectileSpawnerAuthoring authoring)
           {
               var spawnerEntity = GetEntity(TransformUsageFlags.None);

               
               AddComponent(spawnerEntity, new ProjectileSpawnerData()
               {
                   ProjectileToSpawn = GetEntity(authoring.projectile.ProjectilePrefab, TransformUsageFlags.Dynamic)
               });
           }
       }

       private struct ProjectileSpawnerData : IComponentData
       {
           public Entity ProjectileToSpawn;
       }
    }
}
