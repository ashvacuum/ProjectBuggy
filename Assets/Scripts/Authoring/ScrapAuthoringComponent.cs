using Authoring.Projectiles;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class ScrapAuthoringComponent : MonoBehaviour
    {
        public float TimeToReachTarget = .5f;
        public float ScrapToGive = 1f;  
    
        class Baker : Baker<ScrapAuthoringComponent>
        {
            public override void Bake(ScrapAuthoringComponent authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ScrapComponent()
                {
                    IsMovingTowardsTarget = false,
                    TimeLeft = 0,
                    TimeToReachTarget = authoring.TimeToReachTarget,
                    ScrapToGive = authoring.ScrapToGive
                });
                AddComponent<LocalTransform>(entity);
                AddComponent<NewSpawnRenderInvisibleTag>(entity);
            }
        }
    }


    public struct ScrapComponent : IComponentData
    {
        public float TimeToReachTarget;
        public float TimeLeft;
        public float ScrapToGive;
        public bool IsMovingTowardsTarget;
    }
}