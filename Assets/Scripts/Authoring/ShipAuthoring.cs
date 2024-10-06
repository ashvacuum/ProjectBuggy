using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
        public float Health = 400f;
        public float NextTimeCanTakeDamage = 0.4f;
        

        public class Baker : Baker<ShipAuthoring>
        {
            public override void Bake(ShipAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShipComponent()
                {
                    ForwardSpeed = authoring.speed
                });
                AddComponent(entity, new CameraFollow()
                {
                    Offset = authoring.CameraOffset,
                    CameraPitch = authoring.CameraPitchOverride,
                    CameraSpeed = authoring.CameraSpeedOverride
                });
                AddComponent(entity, new HealthComponent()
                {
                    Health = authoring.Health,
                    NextTimeToTakeDamage = authoring.NextTimeCanTakeDamage,
                    CurrentNextTimeToTakeDamage = 0
                });


            }
        }
    }

    public struct ShipComponent : IComponentData
    {
        public float ForwardSpeed;
    }

    public struct HealthComponent : IComponentData
    {
        public float Health;
        public float NextTimeToTakeDamage;
        public float CurrentNextTimeToTakeDamage;
    }

    public struct DamageComponent : IComponentData
    {
        public float Damage;
    }
}
