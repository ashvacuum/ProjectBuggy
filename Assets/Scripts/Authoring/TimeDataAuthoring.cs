using Unity.Collections;
using Unity.Entities;
using UnityEngine;



public class TimeDataAuthoring : MonoBehaviour
{
    public TimeData timeData;

    public class Baker : Baker<TimeDataAuthoring>
    {
        public override void Bake(TimeDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            // Create blob asset
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<TimeDataBlobAsset>();
            
            root.maxTimeInMinutes = authoring.timeData.maxTimeInMinutes;


            // Add component that will hold the blob asset reference
            var blobAssetReference = builder.CreateBlobAssetReference<TimeDataBlobAsset>(Allocator.Persistent);

            AddComponent(entity, new TimeDataComponent { TimeBlob = blobAssetReference });
            
            // Add TimeManagerComponent to the same entity
            AddComponent(entity, new TimeManagerComponent 
            { 
                CurrentTime = 0f,
                IsRunning = true,
                IsPaused = false
            });
            
            builder.Dispose();
        }
    }
}

public struct TimeDataComponent : IComponentData
{
    public BlobAssetReference<TimeDataBlobAsset> TimeBlob;
}

public struct TimeManagerComponent : IComponentData
{
    public float CurrentTime;
    public bool IsRunning;
    public bool IsPaused;
}