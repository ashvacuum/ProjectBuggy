using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{


    public struct KnockBackReceiver : IComponentData
    {
        public float knockbackForce;
        public float recoveryTime;
        public float currentRecoveryTime;
        public float3 currentKnockbackVelocity;
        public bool isBeingKnockedBack;
    }

    public struct KnockbackSender : IComponentData
    {
        public float knockbackForceToSend;
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(DamageCollisionSystem))]
    public partial struct KnockBackSystem : ISystem
    {
        
        private ComponentLookup<LocalTransform> _transform;
        private ComponentLookup<KnockBackReceiver> _knockbackReceiver;
        private ComponentLookup<PhysicsMass> _massOverride;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _transform = SystemAPI.GetComponentLookup<LocalTransform>();
            _knockbackReceiver = SystemAPI.GetComponentLookup<KnockBackReceiver>();
            _massOverride = SystemAPI.GetComponentLookup<PhysicsMass>();
        }

        public void OnUpdate(ref SystemState state)
        {
            _transform.Update(ref state);
            _knockbackReceiver.Update(ref state);
            _massOverride.Update(ref state);
            
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, knockBack, physicsBody) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<KnockBackReceiver>,
                         RefRO<PhysicsMass>>())
            {
                var isKinematic = physicsBody.ValueRO.IsKinematic;
                Debug.Log("Knocking back");
                if (knockBack.ValueRW.currentRecoveryTime <= 0) continue;
                if (knockBack.ValueRW.isBeingKnockedBack)
                {
                    if (isKinematic)
                    {
                        // Kinematic body: Update transform directly
                        transform.ValueRW.Position += knockBack.ValueRW.currentKnockbackVelocity * deltaTime;
                        knockBack.ValueRW.currentKnockbackVelocity *= math.exp(-5f * deltaTime);
                        Debug.Log("Knocking back");
                        if (math.lengthsq(knockBack.ValueRW.currentKnockbackVelocity) < 0.01f)
                        {
                            knockBack.ValueRW.isBeingKnockedBack = false;
                            knockBack.ValueRW.currentKnockbackVelocity = float3.zero;
                        }
                    }
                    // For non-kinematic bodies, the physics system handles the movement
                }

                knockBack.ValueRW.currentRecoveryTime -= deltaTime;
            }
            
            foreach (var (collisionBuffer,knockbackForceSent,entityA) in 
                     SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>,RefRO<KnockbackSender>>().WithEntityAccess())
            {
                
                if (collisionBuffer.Length == 0 || knockbackForceSent.ValueRO.knockbackForceToSend <= 0) 
                    continue;

                foreach (var collisionEventBuffer in collisionBuffer)
                {
                    
                    var entityB = collisionEventBuffer.GetOtherEntity(entityA);
                    var knockback = _knockbackReceiver[entityB];
                    var collision = collisionEventBuffer;
                    if (knockback.currentRecoveryTime > 0 || collision.State != StatefulEventState.Enter) continue;
                    
                    var isKinematic = _massOverride[entityB].IsKinematic;

                    var knockbackDirection =
                        math.normalize(_transform[entityA].Position - _transform[entityB].Position);
                    var knockbackVelocity = knockbackDirection * knockbackForceSent.ValueRO.knockbackForceToSend;

                    if (isKinematic)
                    {
                        // Kinematic body: Store velocity for transform updates
                        knockback.currentKnockbackVelocity = knockbackVelocity;
                        knockback.isBeingKnockedBack = true;
                    }

                    knockback.currentRecoveryTime = knockback.recoveryTime;
                    
                    
                }
            }

        }
    }
}
