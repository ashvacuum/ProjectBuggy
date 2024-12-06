using Authoring.Projectiles;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateBefore(typeof(ProjectileMovementSystem))]
    [UpdateBefore(typeof(ScrapMovementSystem))]
    partial struct RenderingFlickerFixSystem : ISystem
    {
        private EntityQuery _query;
    
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _query = state.EntityManager.CreateEntityQuery(
                ComponentType.ReadWrite<NewSpawnRenderInvisibleTag>(),
                ComponentType.Exclude<DeadComponentTag>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var entity in _query.ToEntityArray(Allocator.Temp))
            {
                if (state.EntityManager.HasComponent<RenderFilterSettings>(entity))
                {
                    var renderSettings = state.EntityManager.GetSharedComponent<RenderFilterSettings>(entity);
                    renderSettings.Layer = 0; // Hardcoded to layer 0, adjust as needed
                    ecb.SetSharedComponent(entity, renderSettings);
                    Debug.Log("Resetting Entity Render Settings");
                }
                
                // Recursively change layers for all nested children
                ChangeLayerRecursivelyDeep(ref state, ecb, entity, 0);
                
                
                // Remove the NewEnemySpawn tag
                ecb.RemoveComponent<NewSpawnRenderInvisibleTag>(entity);
            }
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
                    Debug.Log("Resetting Entity Render Settings");
                }

                // Recursively process THIS child's children (key difference)
                // This ensures we go as deep as the hierarchy goes
                ChangeLayerRecursivelyDeep(ref state, ecb, childEntity, newRenderLayer);
            }
        }
    }
}
