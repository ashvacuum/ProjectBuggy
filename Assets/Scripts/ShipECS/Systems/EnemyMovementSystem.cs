using System;
using Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
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
                    DeltaTime = SystemAPI.Time.DeltaTime
                }.ScheduleParallel();
            }
        }
    }
    
    [WithAll(typeof(EnemyFollowTarget), typeof(KnockBackReceiver))]
    [WithNone(typeof(NewEnemySpawn))]
    [BurstCompile]
    public partial struct FollowPlayerJob : IJobEntity
    {
        public float3 TargetLocation;
        public float DeltaTime;

        void Execute(ref LocalTransform shipTransform, EnemyFollowTarget followTarget, KnockBackReceiver receiver)
        {
            
            if (math.distance(shipTransform.Position,TargetLocation) < followTarget.FollowTargetLimits || receiver.isBeingKnockedBack)
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
