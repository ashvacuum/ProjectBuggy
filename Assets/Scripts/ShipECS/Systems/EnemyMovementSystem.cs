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
                         .WithAll<ShipComponent>()
                         .WithEntityAccess())
            {
                new FollowPlayerJob()
                {
                    TargetLocation = playerTransform.ValueRO.Position,
                    DeltaTime = SystemAPI.Time.DeltaTime
                }.ScheduleParallel();
                /*
                foreach (var (enemyTransform, followComponent, entity) in
                         SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyFollowTarget>>()
                             .WithNone<NewEnemySpawn>()
                             .WithEntityAccess())
                {
                    
                    if (math.distance(enemyTransform.ValueRO.Position,playerTransform.ValueRO.Position) < 10f)
                    {
                        return;
                    }
                    
                    var calcExp = SystemAPI.Time.DeltaTime * followComponent.ValueRO.Speed;
                    enemyTransform.ValueRW.Position = math.lerp(
                        enemyTransform.ValueRO.Position,
                        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                        enemyTransform.ValueRO.Position + enemyTransform.ValueRO.Forward(),
                        math.pow(calcExp, 1f/3f));
                    
                    var direction = playerTransform.ValueRO.Position - enemyTransform.ValueRO.Position;
                    enemyTransform.ValueRW.Rotation = quaternion.LookRotation(
                        math.normalize(direction)  * SystemAPI.Time.DeltaTime * followComponent.ValueRO.RotationSpeed,
                        math.up());

                    
                }*/
            }
        }
    }
    
    [WithAll(typeof(EnemyFollowTarget))]
    [WithNone(typeof(NewEnemySpawn))]
    [BurstCompile]
    public partial struct FollowPlayerJob : IJobEntity
    {
        public float3 TargetLocation;
        public float DeltaTime;

        void Execute([EntityIndexInQuery] int entityIndex, Entity entity, ref LocalTransform shipTransform, ref EnemyFollowTarget followTarget)
        {
            if (math.distance(shipTransform.Position,TargetLocation) < 5f)
            {
                return;
            }
            
            var calcExp = DeltaTime * followTarget.Speed;
            shipTransform.Position = math.lerp(
                shipTransform.Position,
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                shipTransform.Position + shipTransform.Forward(),
                math.pow(calcExp, 1f/3f));
            
            var direction = TargetLocation - shipTransform.Position;
            shipTransform.Rotation = quaternion.LookRotation(
                math.normalize(direction)  * DeltaTime * followTarget.RotationSpeed,
                math.up());
            
            
        }
    }
}
