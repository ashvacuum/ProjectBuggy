using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
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

    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [UpdateBefore(typeof(EnemyMovementSystem))]
    public partial struct KnockBackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
/*
            var jobUpdateKnockBack = new KnockbackSendJob()
            {
                Transform = SystemAPI.GetComponentLookup<LocalTransform>(),
                KnockBackReceiver = SystemAPI.GetComponentLookup<KnockBackReceiver>(),
                MassOverride = SystemAPI.GetComponentLookup<PhysicsMass>(true),
                ElapsedTime = (float)SystemAPI.Time.ElapsedTime
            };

            jobUpdateKnockBack.Schedule();*/

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (knockbackReceiver,mass,enemyTransform,entityB) in SystemAPI.Query<RefRW<KnockBackReceiver>, RefRO<PhysicsMass>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (!mass.ValueRO.IsKinematic || knockbackReceiver.ValueRO.currentRecoveryTime > 0) continue;
                foreach (var (sender,triggerEvents,projectileTransform,entityA) in SystemAPI.Query<RefRO<KnockbackSender>,DynamicBuffer<StatefulTriggerEvent>,RefRO<LocalTransform>>().WithEntityAccess())
                {
                    foreach (var triggerEvent in triggerEvents)
                    {
                        if (triggerEvent.GetOtherEntity(entityA) != entityB || triggerEvent.State != StatefulEventState.Enter) continue;

                        var knockbackDirection =
                            math.normalize(projectileTransform.ValueRO.Position - enemyTransform.ValueRO.Position) * -1;

                        var knockbackVelocity = knockbackDirection * sender.ValueRO.knockbackForceToSend;
                        
                        ecb.SetComponent(entityB, new KnockBackReceiver()
                        {
                            currentKnockbackVelocity = knockbackVelocity,
                            currentRecoveryTime = knockbackReceiver.ValueRO.recoveryTime,
                            isBeingKnockedBack = true,
                            knockbackForce = 1,
                            recoveryTime = knockbackReceiver.ValueRO.recoveryTime
                        });

                    }
                }
            }
            //Debug.Log($"Knockback initiated {SystemAPI.Time.ElapsedTime}");

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            
            

        }
    }
    
    [BurstCompile]
    public partial struct KnockbackSendJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> Transform;
        [ReadOnly] public ComponentLookup<PhysicsMass> MassOverride;
        public ComponentLookup<KnockBackReceiver> KnockBackReceiver;
        public float ElapsedTime;

        void Execute(Entity entityA, in KnockbackSender sender, in PhysicsMass mass,
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
