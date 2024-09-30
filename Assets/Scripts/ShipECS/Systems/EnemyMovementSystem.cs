using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
                             .WithAll<EnemyFollowTarget>()
                             .WithNone<NewEnemySpawn>()
                             .WithEntityAccess())
                {
                    //stop if you get too close
                    if (math.distance(enemyTransform.ValueRO.Position,playerTransform.ValueRO.Position) < 10f)
                    {
                        return;
                    }

                    var calcExp = SystemAPI.Time.DeltaTime * followComponent.ValueRO.Speed;
                    enemyTransform.ValueRW.Position = math.lerp(
                        enemyTransform.ValueRO.Position,
                        playerTransform.ValueRO.Position,
                        math.pow(calcExp, 1f/3f));

                    //TODO: rotate towards direction
                    //TODO: move them towards their forward direction and them rotate them towards enemy slowly.
                    

                }
            }
        }
    }
}
