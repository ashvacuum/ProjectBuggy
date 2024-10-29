using Unity.Entities;
using Unity.Burst;
using Unity.Core;
using UnityEngine;

[BurstCompile]
public partial struct TimeManagerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TimeDataComponent>();
        state.RequireForUpdate<TimeManagerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var timeData = SystemAPI.GetSingleton<TimeDataComponent>();
        var timeManager = SystemAPI.GetSingletonRW<TimeManagerComponent>();


            ref var blobAsset = ref timeData.TimeBlob.Value;
            
            // Update current time
            timeManager.ValueRW.CurrentTime += deltaTime;
        var maxTimeInSeconds = blobAsset.maxTimeInMinutes * 60;
            // Clamp to max time
            if (timeManager.ValueRO.CurrentTime >= maxTimeInSeconds)
            {
                timeManager.ValueRW.CurrentTime = blobAsset.maxTimeInMinutes;
                timeManager.ValueRW.IsRunning = false;
            }
        
    }
}