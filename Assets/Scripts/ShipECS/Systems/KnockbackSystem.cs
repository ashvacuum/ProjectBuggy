using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Transforms;

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

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(DamageCollisionSystem))]
    public partial struct KnockBackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, knockBack, physicsBody, physicsVelocity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<KnockBackReceiver>,
                         RefRO<PhysicsMassOverride>, RefRW<PhysicsVelocity>>())
            {
                var isKinematic = physicsBody.ValueRO.IsKinematic;

                if (!(knockBack.ValueRW.currentRecoveryTime > 0)) continue;
                if (knockBack.ValueRW.isBeingKnockedBack)
                {
                    if (isKinematic == 1)
                    {
                        // Kinematic body: Update transform directly
                        transform.ValueRW.Position += knockBack.ValueRW.currentKnockbackVelocity * deltaTime;
                        knockBack.ValueRW.currentKnockbackVelocity *= math.exp(-5f * deltaTime);

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
            
            
            foreach (var (collisionBuffer, knockback, physicsBody, physicsVelocity) in 
                     SystemAPI.Query<DynamicBuffer<StatefulCollisionEvent>, RefRW<KnockBackReceiver>, 
                         RefRO<PhysicsMassOverride>, RefRW<PhysicsVelocity>>())
            {
                if (collisionBuffer.Length == 0 || knockback.ValueRW.currentRecoveryTime > 0) 
                    continue;

                bool isKinematic = physicsBody.ValueRO.IsKinematic == 1;

                foreach (var collisionEventBuffer in collisionBuffer)
                {
                    var collision = collisionEventBuffer;
                    var knockbackDirection = collision.CollisionDetails.;
                    var knockbackVelocity = knockbackDirection * knockback.ValueRW.knockbackForce * collision.impulse;

                    if (isKinematic)
                    {
                        // Kinematic body: Store velocity for transform updates
                        knockback.ValueRW.currentKnockbackVelocity = knockbackVelocity;
                        knockback.ValueRW.isBeingKnockedBack = true;
                    }
                    else
                    {
                        // Non-kinematic body: Apply immediate physics velocity
                        physicsVelocity.ValueRW.Linear += knockbackVelocity;
                    }

                    knockback.ValueRW.currentRecoveryTime = knockback.ValueRW.recoveryTime;
                }
            }

        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
