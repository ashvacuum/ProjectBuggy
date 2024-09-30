using System;
using Authoring;
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

        public void OnUpdate(ref SystemState state)
        {
            
            foreach (var (playerTransform, playerEntity) in
                     SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<ShipComponent>()
                         .WithEntityAccess())
            {
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
                    enemyTransform.ValueRW.Rotation = quaternion.LookRotation(math.normalize(direction)  * SystemAPI.Time.DeltaTime * followComponent.ValueRO.RotationSpeed
                        , math.up());
                }
            }
        }

        
    }
}
