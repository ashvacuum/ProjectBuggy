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
        public float InitialPickupradius = 300f;

        public GameObject BulletPrefab;
        public float BaseProjectileLifetime = 5f;
        public int InitialWeaponCount = 1;
        public int InitialPenetration = 0;
        public float InitialSpeed = 10;
        public float FireRate = 3f;
        

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
                AddComponent(entity, new PickupRadiusComponent()
                {
                    BasePickupRadius = authoring.InitialPickupradius,
                    PickupRadiusBonus = 0f
                });/*
                var bullet = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic);
                AddComponent(bullet, new LocalTransform());
                AddComponent(entity, new PickupRadiusComponent()
                {
                    BasePickupRadius = authoring.InitialPickupradius,
                    PickupRadiusBonus = 0f
                });*/


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

    public struct PickupRadiusComponent : IComponentData
    {
        public float BasePickupRadius;
        public float PickupRadiusBonus; //add to this value over time but starts at 0;
        public float TotalPickupRadius => BasePickupRadius + (PickupRadiusBonus/100 * PickupRadiusBonus);
    }

    public struct DamageComponent : IComponentData
    {
        public float Damage;
    }
}
