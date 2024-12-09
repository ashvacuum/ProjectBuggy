using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Transforms;

public static class RenderingFlickerFixUtil
{
    /// <summary>
    /// Allows you to change render layers of entities including children
    /// </summary>
    /// <param name="state">State in an ISystem</param>
    /// <param name="entity">Target Entity you want to change rendering</param>
    /// <param name="ecb">Entity Command buffer in use</param>
    /// <param name="newRenderLayer"></param>
    /// <typeparam name="T">should refer to an IComponentData tag that pertains to the object being invisible</typeparam>
    public static void BeginRecursiveLayerChange<T>(ref SystemState state, Entity entity, EntityCommandBuffer ecb, int newRenderLayer = 0)
    {
        
        if (state.EntityManager.HasComponent<RenderFilterSettings>(entity))
        {  
            var renderSettings = state.EntityManager.GetSharedComponent<RenderFilterSettings>(entity);
            renderSettings.Layer = 0; // Hardcoded to layer 0, adjust as needed
            ecb.SetSharedComponent(entity, renderSettings);
        }

        // Recursively change layers for all nested children
        ChangeLayerRecursivelyDeep(ref state, ecb, entity, 0);
                

        // Remove the NewEnemySpawn tag
        ecb.RemoveComponent<T>(entity);
    }
    private static void ChangeLayerRecursivelyDeep(ref SystemState state, EntityCommandBuffer ecb, Entity currentEntity, int newRenderLayer)
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
                //Debug.Log("Resetting Entity Render Settings");
            }

            // Recursively process THIS child's children (key difference)
            // This ensures we go as deep as the hierarchy goes
            ChangeLayerRecursivelyDeep(ref state, ecb, childEntity, newRenderLayer);
        }
    }
}
