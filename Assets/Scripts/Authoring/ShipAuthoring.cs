using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Authoring
{
    public class ShipAuthoring : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public float speed = 100f;
        public float3 CameraOffset;
        public float CameraPitchOverride;
        public float CameraSpeedOverride;
        

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
                AddComponent(entity, new CameraFollow()
                {
                    Offset = authoring.CameraOffset,
                    CameraPitch = authoring.CameraPitchOverride,
                    CameraSpeed = authoring.CameraSpeedOverride
                });
                

            }
        }
    }

    public struct ShipComponent : IComponentData
    {
        public float forwardSpeed;
    }
}
