using ShipECS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Authoring
{
    public class ShipAuthoring : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public float speed = 100f;
        public float3 CameraOffset;
        public float CameraPitchOverride;
        public float CameraSpeedOverride;
        [FormerlySerializedAs("Health")] public float MaxHealth = 400f;
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
                    MaxHealth = authoring.MaxHealth,
                    NextTimeToTakeDamage = authoring.NextTimeCanTakeDamage,
                    CurrentNextTimeToTakeDamage = 0,
                    CurrentHealth = authoring.MaxHealth,
                    PreviousHealth = authoring.MaxHealth
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
        public float CurrentHealth;
        public float MaxHealth;
        public float PreviousHealth;
        public float NextTimeToTakeDamage;
        public float CurrentNextTimeToTakeDamage;

        public float HealthPercent => CurrentHealth / MaxHealth * 100;
    }

    public struct DamageComponent : IComponentData
    {
        public float Damage;
    }
}
