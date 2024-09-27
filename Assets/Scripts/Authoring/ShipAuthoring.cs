using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class ShipAuthoring : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public float speed = 100f;

        public class Baker : Baker<ShipAuthoring>
        {
            public override void Bake(ShipAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShipComponent()
                {
                    forwardSpeed = authoring.speed
                });
                AddComponent(entity, new LocalTransform());

            }
        }
    }

    public struct ShipComponent : IComponentData
    {
        public float forwardSpeed;
    }
}
