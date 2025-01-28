using Authoring;
using Authoring.Projectiles;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

partial struct RocketTrailInitializeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VFXThrustersSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        VFXThrustersSingleton vfxThrustersSingleton = SystemAPI.GetSingletonRW<VFXThrustersSingleton>().ValueRW;
        
        ShipInitializeJob shipInitializeJob = new ShipInitializeJob
        {
            ThrustersManager = vfxThrustersSingleton.Manager,
        };

        state.Dependency = shipInitializeJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
[WithAll(typeof(NewSpawnRenderInvisibleTag))]
public partial struct ShipInitializeJob : IJobEntity
{
    public VFXManagerParented<VFXRocketData> ThrustersManager;
            
    private void Execute(ref RocketTrailData rocket)
    {
        RocketData shipData = rocket.RocketData.Value;

        rocket.RocketVFXIndex = ThrustersManager.Create();
                
        if (rocket.RocketVFXIndex >= 0)
        {
            ThrustersManager.Datas[rocket.RocketVFXIndex] = new VFXRocketData()
            {
                Color = new float3(0,0,1), //blue
                Size = shipData.ThrusterSize,
                Length = shipData.ThrusterLength,
            };
        }
    }
}
