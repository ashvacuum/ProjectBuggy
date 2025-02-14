using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Obsolete]
public class ProjectileAuthoring : MonoBehaviour
{
    class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<Projectile>(entity);


        }
    }
}

public struct Projectile : IComponentData
{
    public float3 Velocity;
}