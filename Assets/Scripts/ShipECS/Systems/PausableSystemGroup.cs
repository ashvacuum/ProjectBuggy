using Unity.Entities;
using Unity.Physics.Systems;

namespace ShipECS.Systems
{
    public partial class PausableSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<TimeManagerComponent>())
            {
                var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();
                if (pauseState.IsPaused)
                    return;

            }
            
            base.OnUpdate();
        }
    }
    
    public partial class PrePhysicsPausableSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<TimeManagerComponent>())
            {
                var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();
                if (pauseState.IsPaused)
                    return;

            }
            
            base.OnUpdate();
        }
    }
    
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial class PostPhysicsPausableSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<TimeManagerComponent>())
            {
                var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();
                if (pauseState.IsPaused)
                    return;

            }
            
            base.OnUpdate();
        }
    }
    
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial class InPhysicsPausableSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<TimeManagerComponent>())
            {
                var pauseState = SystemAPI.GetSingleton<TimeManagerComponent>();
                if (pauseState.IsPaused)
                    return;

            }
            
            base.OnUpdate();
        }
    }
}
