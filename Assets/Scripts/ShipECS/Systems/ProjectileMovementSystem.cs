using Authoring;
using Authoring.Projectiles;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct ProjectileMovementSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ProjectileMotion>();
            _query = state.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NewSpawnRenderInvisibleTag>(), ComponentType.Exclude<DeadComponentTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            
            foreach (var (projectile,entity) in SystemAPI.Query<ProjectileAspect>().WithNone<DeadComponentTag, NewSpawnRenderInvisibleTag>().WithEntityAccess())
            {
                if (!projectile.IsAlive)
                {
                    ecb.AddComponent<DeadComponentTag>(entity);
                    continue;
                }
                
                projectile.Position = math.lerp(projectile.Position,
                    projectile.Position + projectile.ForwardVector,
                    Time.deltaTime * projectile.Speed);
                projectile.Lifetime -= SystemAPI.Time.DeltaTime;
                //Debug.DrawRay(projectile.Position,(projectile.Position + projectile.ForwardVector), Color.red, .1f );
                
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
        private void ChangeLayerRecursivelyDeep(ref SystemState state, EntityCommandBuffer ecb, Entity currentEntity, int newRenderLayer)
        {
            if (!state.EntityManager.HasBuffer<Child>(currentEntity)) return;
            var children = state.EntityManager.GetBuffer<Child>(currentEntity);
            for (int i = 0; i < children.Length; i++)
            {
                Entity childEntity = children[i].Value;

                // Change render layer for this child
                if (state.EntityManager.HasComponent<RenderFilterSettings>(childEntity))
                {
                    var renderSettings = state.EntityManager.GetSharedComponent<RenderFilterSettings>(childEntity);
                    renderSettings.Layer = newRenderLayer;
                    ecb.SetSharedComponent(childEntity, renderSettings);
                }

                // Recursively process THIS child's children (key difference)
                // This ensures we go as deep as the hierarchy goes
                ChangeLayerRecursivelyDeep(ref state, ecb, childEntity, newRenderLayer);
            }
        }
    }

    public readonly partial struct ProjectileAspect : IAspect
    {
        private readonly RefRW<ProjectileMotion> _motion;
        private readonly RefRO<HealthComponent> _health;
        private readonly RefRW<LocalTransform> _transform;
        public bool IsAlive => _health.ValueRO.CurrentHealth > 0 && _motion.ValueRW.LifeTime > 0;
        public float3 ForwardVector => _motion.ValueRO.Direction;
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


