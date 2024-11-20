using System;
using Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PostPhysicsPausableSystemGroup))]
    [UpdateAfter(typeof(KnockBackSystem))]
    public partial struct EnemyMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyFollowTarget>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            foreach (var (playerTransform, playerEntity) in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                new FollowPlayerJob()
                {
                    TargetLocation = playerTransform.ValueRO.Position,
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    ElapsedTime = (float)SystemAPI.Time.ElapsedTime
                }.ScheduleParallel();
            }
        }
    }
    
    [WithAll(typeof(EnemyFollowTarget), typeof(KnockBackReceiver))]
    [WithNone(typeof(NewEnemySpawn), typeof(DeadComponentTag))]
    [BurstCompile]
    public partial struct FollowPlayerJob : IJobEntity
    {
        public float3 TargetLocation;
        public float DeltaTime;
        public float ElapsedTime;

        void Execute(ref LocalTransform shipTransform, ref KnockBackReceiver receiver, in EnemyFollowTarget followTarget, in PhysicsMass mass)
        {
            if (receiver.isBeingKnockedBack)
            {
                var isKinematic = mass.IsKinematic;
                if (receiver is { isBeingKnockedBack: true } && isKinematic)
                {
                    // Kinematic body: Update transform directly
                    //Debug.Log($"Knockback Movement {ElapsedTime}");
                    shipTransform.Position += receiver.currentKnockbackVelocity * DeltaTime;
                    receiver.currentKnockbackVelocity *= math.exp(-5f * DeltaTime);
                
                    if (math.lengthsq(receiver.currentKnockbackVelocity) < 0.01f || receiver.currentRecoveryTime <= 0)
                    {
                        receiver.isBeingKnockedBack = false;
                    
                        receiver.currentKnockbackVelocity = float3.zero;
                    }

                }
            

                receiver.currentRecoveryTime -= DeltaTime;
                return;
            }
            
            if (math.distance(shipTransform.Position,TargetLocation) < followTarget.FollowTargetLimits )
            {
                return;
            }
            
            var calcExp = DeltaTime * followTarget.Speed;
            var calculatedPosition = math.lerp(
                shipTransform.Position,
                shipTransform.Position + shipTransform.Forward(),
                calcExp);
            
            shipTransform.Position = new float3(calculatedPosition.x, 0, calculatedPosition.z);
            
            var direction = TargetLocation - shipTransform.Position;
            shipTransform.Rotation = quaternion.LookRotation(
                math.normalize(direction)  * DeltaTime * followTarget.RotationSpeed,
                math.up());
            
            
        }
    }
}
