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
        public float recoveryTime;
        public float currentRecoveryTime;
        public float3 currentKnockbackVelocity;
        public bool isBeingKnockedBack;
    }

    public struct KnockbackSender : IComponentData
    {
        public float knockbackForceToSend;
    }

    [UpdateInGroup(typeof(PostPhysicsPausableSystemGroup))]
    [UpdateAfter(typeof(Damage.DamageCollisionSystem))]
    public partial struct KnockBackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            var jobUpdateKnockBack = new KnockbackSendJob()
            {
                Transform = SystemAPI.GetComponentLookup<LocalTransform>(),
                KnockBackReceiver = SystemAPI.GetComponentLookup<KnockBackReceiver>(),
                MassOverride = SystemAPI.GetComponentLookup<PhysicsMass>(true),
                ElapsedTime = (float)SystemAPI.Time.ElapsedTime
            };

            jobUpdateKnockBack.Schedule();
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
                if (entityB == Entity.Null) continue;
                if (!KnockBackReceiver.HasComponent(entityB) || !Transform.HasComponent(entityA) || !Transform.HasComponent(entityB)) continue;
                var knockBack = KnockBackReceiver[entityB];
                
                var collision = collisionEventBuffer;
                if (knockBack.currentRecoveryTime > 0 || collision.State != StatefulEventState.Enter) continue;

                var isKinematic = MassOverride[entityB].IsKinematic;
                
                var knockBackDirection =
                     -1f * math.normalize(Transform[entityA].Position - Transform[entityB].Position);
                var knockBackVelocity = knockBackDirection * sender.knockbackForceToSend;

                if (isKinematic)
                {
                    
                    // Kinematic body: Store velocity for transform updates
                    knockBack.currentKnockbackVelocity = knockBackVelocity;
                    knockBack.isBeingKnockedBack = true;
                }

                knockBack.currentRecoveryTime = knockBack.recoveryTime;

                KnockBackReceiver[entityB] = knockBack;
            }
        }
    }
}
