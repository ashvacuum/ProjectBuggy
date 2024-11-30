using ShipECS.Systems;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public float speed = 20f;
        public float rotationSpeed = 20f;
        public float damage = 1;
        public float targetRangeBounds = 50f;
        public float maxHealth = 10f;
        public float nextDamageDelay = .2f;
        public float knockBackRecoveryTime = .5f;
        private class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring) 
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NewEnemySpawn());
                AddComponent(entity, new DisableRendering());
                AddSharedComponent(entity, new EnemyFollowTarget()
                {
                    Speed = authoring.speed,
                    RotationSpeed = authoring.rotationSpeed,
                    FollowTargetLimits = authoring.targetRangeBounds
                });
                AddComponent(entity, new DamageComponent()
                {
                    Damage = authoring.damage
                });
                AddComponent(entity, new HealthComponent()
                {
                    MaxHealth = authoring.maxHealth,
                    CurrentHealth = authoring.maxHealth,
                    CurrentNextTimeToTakeDamage = 0f,
                    NextTimeToTakeDamage = authoring.nextDamageDelay,
                    PreviousHealth = authoring.maxHealth
                });
                AddComponent(entity, new KnockBackReceiver()
                {
                    currentKnockbackVelocity = float3.zero,
                    isBeingKnockedBack = false,
                    recoveryTime = authoring.knockBackRecoveryTime,
                    currentRecoveryTime = 0
                });
            }
        }
    }

    public struct EnemyFollowTarget : ISharedComponentData
    {
        public float Speed;
        public float RotationSpeed;
        public float FollowTargetLimits;
        
    }

    public struct NewEnemySpawn : IComponentData { }
}
