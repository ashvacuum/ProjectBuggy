using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct ProjectileMotionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProjectileMotion>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var projectile in SystemAPI.Query<ProjectileAspect>())
            {
                if (!projectile.IsAlive ) continue;
                
                projectile.Position = math.lerp(projectile.Position, projectile.Position + projectile.ForwardVector
                    ,
                    Time.deltaTime * projectile.Speed);
                projectile.Lifetime -= SystemAPI.Time.DeltaTime;
            }
        }
    }

    public readonly partial struct ProjectileAspect : IAspect
    {
        private readonly RefRW<ProjectileMotion> _motion;
        private readonly RefRO<HealthComponent> _health;
        private readonly RefRW<LocalTransform> _transform;
        public bool IsAlive => _health.ValueRO.CurrentHealth > 0 && _motion.ValueRW.LifeTime > 0;
        public float3 ForwardVector => math.mul(_transform.ValueRO.Rotation, math.up());
        public float Speed => _motion.ValueRO.Speed;
        

        public float Lifetime
        {
            get => _motion.ValueRO.LifeTime;
            set => _motion.ValueRW.LifeTime = value;
        }
        
        public float3 Position
        {
            get => _transform.ValueRO.Position;
            set => _transform.ValueRW.Position = value;
        }
    }

    public struct ProjectileMotion : IComponentData, IEnableableComponent
    {
        public float3 Direction;
        public float Speed;
        public float LifeTime;
    }
}


