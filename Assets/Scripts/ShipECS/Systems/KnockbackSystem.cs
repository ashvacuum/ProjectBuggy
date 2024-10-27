using Authoring;
using Unity.Burst;
using Unity.Collections;
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
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _transform.Update(ref state);
            _knockbackReceiver.Update(ref state);
            _massOverride.Update(ref state);
            
            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new KnockBackMoveJob()
            {
                deltaTime = deltaTime
            };
            job.ScheduleParallel();
            
            /*
            foreach (var (transform, knockBack, physicsBody) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<KnockBackReceiver>,
                         RefRO<PhysicsMass>>())
            {
                var isKinematic = physicsBody.ValueRO.IsKinematic;
                
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
            }*/

            var jobUpdateKnockBack = new KnockbackSendJob()
            {
                Transform = SystemAPI.GetComponentLookup<LocalTransform>(),
                KnockBackReceiver = SystemAPI.GetComponentLookup<KnockBackReceiver>(),
                MassOverride = SystemAPI.GetComponentLookup<PhysicsMass>(true)
            };

            jobUpdateKnockBack.Schedule();
            /*
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
                        Debug.Log("Knockback initiated");
                        // Kinematic body: Store velocity for transform updates
                        knockback.currentKnockbackVelocity = knockbackVelocity;
                        knockback.isBeingKnockedBack = true;
                    }

                    knockback.currentRecoveryTime = knockback.recoveryTime;
                }
            }*/

        }
    }
    
    [BurstCompile]
    [WithNone(typeof(NewEnemySpawn))]
    public partial struct KnockBackMoveJob : IJobEntity
    {
        public float deltaTime;
        void Execute(ref LocalTransform transform, ref KnockBackReceiver receiver, PhysicsMass mass)
        {
            var isKinematic = mass.IsKinematic;
            
            if (receiver is { currentRecoveryTime: <= 0, isBeingKnockedBack: true } && isKinematic)
            {
                // Kinematic body: Update transform directly
                transform.Position += receiver.currentKnockbackVelocity * deltaTime;
                receiver.currentKnockbackVelocity *= math.exp(-5f * deltaTime);
                Debug.Log($"Moving back {math.lengthsq(receiver.currentKnockbackVelocity)}");
                
                if (math.lengthsq(receiver.currentKnockbackVelocity) < 0.01f)
                {
                    receiver.isBeingKnockedBack = false;
                    
                    receiver.currentKnockbackVelocity = float3.zero;
                }

            }
            // For non-kinematic bodies, the physics system handles the movement
            

            receiver.currentRecoveryTime -= deltaTime;
        }
    }
    
    [BurstCompile]
    public partial struct KnockbackSendJob : IJobEntity
    {
        public ComponentLookup<LocalTransform> Transform;
        [ReadOnly] public ComponentLookup<PhysicsMass> MassOverride;
        public ComponentLookup<KnockBackReceiver> KnockBackReceiver;
        

        void Execute(Entity entityA, ref KnockbackSender sender, in PhysicsMass mass,
            in DynamicBuffer<StatefulTriggerEvent> statefulTriggerEvents)
        {
            if (statefulTriggerEvents.Length == 0 || sender.knockbackForceToSend <= 0)
                return;

            foreach (var collisionEventBuffer in statefulTriggerEvents)
            {

                var entityB = collisionEventBuffer.GetOtherEntity(entityA);
                var knockback = KnockBackReceiver[entityB];
                var collision = collisionEventBuffer;
                if (knockback.currentRecoveryTime > 0 || collision.State != StatefulEventState.Enter) continue;

                var isKinematic = MassOverride[entityB].IsKinematic;

                var knockbackDirection =
                     -1f * math.normalize(Transform[entityA].Position - Transform[entityB].Position);
                var knockbackVelocity = knockbackDirection * sender.knockbackForceToSend;

                if (isKinematic)
                {
                    Debug.Log($"Knockback initiated");
                    // Kinematic body: Store velocity for transform updates
                    knockback.currentKnockbackVelocity = knockbackVelocity;
                    knockback.isBeingKnockedBack = true;
                }

                knockback.currentRecoveryTime = knockback.recoveryTime;

                KnockBackReceiver[entityB] = knockback;
            }
        }
    }
}
