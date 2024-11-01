using System.Collections.Generic;
using System.Linq;
using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateBefore(typeof(ProjectileFiringSystem))]
    public partial struct NearbyEnemyTargetingSystem : ISystem
    {
        
        private EntityQuery _playerQuery;
        private EntityQuery _enemyQuery;

        public void OnCreate(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<EnemyTargetPoints>(entity);
            
            _playerQuery = state.GetEntityQuery(typeof(PlayerTag), typeof(LocalTransform));
            _enemyQuery = state.GetEntityQuery(typeof(EnemyFollowTarget), typeof(LocalTransform), ComponentType.Exclude<NewEnemySpawn>(), ComponentType.Exclude<DeadComponentTag>());
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_playerQuery.IsEmpty) return;
            
            var player = _playerQuery.GetSingletonEntity();
            var playerPos = SystemAPI.GetComponent<LocalTransform>(
                player
            ).Position;

            var enemies = _enemyQuery.ToEntityArray(Allocator.Temp);

            var targetBuffer = SystemAPI.GetSingletonBuffer<EnemyTargetPoints>();
            var transforms = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var sortedTargets = CollectionHelper.CreateNativeArray<EnemyTargetPoints>(
                enemies.Length,
                Allocator.Temp,
                NativeArrayOptions.UninitializedMemory
            );

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemyTransform = transforms[enemies[i]];
                var distance = math.distance(playerPos, enemyTransform.Position);
                
                sortedTargets[i] = new EnemyTargetPoints()
                {
                    Enemy = enemies[i],
                    Distance = distance,
                    Position = enemyTransform.Position
                };
            }
            
            sortedTargets.Sort(new DistanceComparer());

            targetBuffer.AddRange(sortedTargets);

            enemies.Dispose();
/*
            
            var tracking = SystemAPI.GetSingletonRW<EnemyTrackingComponent>();
            tracking.ValueRW.TrackingTargetPosition =
                new float3(100000f, 100000f, 100000f);
            foreach (var playerTransform in SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<PlayerTag>())
            {
                foreach (var enemyTransform in SystemAPI.Query<RefRO<LocalTransform>>()
                             .WithAll<EnemyFollowTarget>()
                             .WithNone<NewEnemySpawn>())
                {
                    var distance = math.distance(enemyTransform.ValueRO.Position, playerTransform.ValueRO.Position);
                    var pastDistance = math.distance(tracking.ValueRO.TrackingTargetPosition, playerTransform.ValueRO.Position);
                    if (pastDistance > distance)
                    {
                        tracking.ValueRW.TrackingTargetPosition = enemyTransform.ValueRO.Position;
                    }
                    
                }

                //Debug.Log($"Tracking {tracking.TrackingTargetPosition.x},{tracking.TrackingTargetPosition.y},{tracking.TrackingTargetPosition.z}");
                break;
            }*/

        }
    }
    
    [BurstCompile]
    public struct DistanceComparer : IComparer<EnemyTargetPoints>
    {
        public int Compare(EnemyTargetPoints x, EnemyTargetPoints y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }

    public struct EnemyTargetPoints : IBufferElementData
    {
            public Entity Enemy;
            public float3 Position;
            public float Distance;
    }
}

