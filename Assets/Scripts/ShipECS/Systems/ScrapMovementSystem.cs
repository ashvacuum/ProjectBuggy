using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    public partial struct ScrapMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<ScrapComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (targetTransform, pickupRadius) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PickupRadiusComponent>>().WithAll<PlayerTag>())
            {
                foreach (var (scrap, transform, entity) in SystemAPI
                             .Query<RefRW<ScrapComponent>, RefRW<LocalTransform>>().WithEntityAccess())
                {
                    // dont move if the player is not nearby or if the scrap is not yet picked up by the player

                    //Debug.Log($"{math.distance(targetTransform.ValueRO.Position, transform.ValueRO.Position)} : {pickupRadius.ValueRO.TotalPickupRadius} : {!scrap.ValueRW.IsMovingTowardsTarget}");

                    if (math.distance(targetTransform.ValueRO.Position, transform.ValueRO.Position) >
                        pickupRadius.ValueRO.TotalPickupRadius && !scrap.ValueRW.IsMovingTowardsTarget) continue;

                    var lerpTime = 0f;

                    if (scrap.ValueRO.TimeLeft >=
                        scrap.ValueRO.TimeToReachTarget || math.distance(targetTransform.ValueRO.Position, transform.ValueRO.Position) < 1f) // destroy entity if it has reached target
                    {
                        ecb.DestroyEntity(entity);
                        var scrapNotifyEntity = ecb.CreateEntity();
                        ecb.AddComponent(scrapNotifyEntity, new ScrapNotify()
                        {
                            ScrapValue = scrap.ValueRO.ScrapToGive
                        });
                        continue;
                    }

                    scrap.ValueRW.IsMovingTowardsTarget = true;
                    lerpTime = (scrap.ValueRO.TimeLeft / scrap.ValueRO.TimeToReachTarget);
                    lerpTime = ComputeEaseIn(lerpTime);

                    //Do easing here
                    var newPos = math.lerp(transform.ValueRO.Position, targetTransform.ValueRO.Position, lerpTime);

                    transform.ValueRW.Position = newPos;

                    scrap.ValueRW.TimeLeft += SystemAPI.Time.DeltaTime;
                }

                break; // do this so it only does it to the first element in this array
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

        }

        private float ComputeEaseIn(float t)
        {
            // Clamp t to ensure it's between 0 and 1
            t = math.clamp(t, 0, 1);

            if (t < 0.75f)
            {
                // Ease out (move away)
                return math.pow(t * 2, 3) / 2; // Scale to [0, 1] for t in [0, 0.5]
            }

            // Ease in (move toward zero)
            return 1 - math.pow((t - 0.5f) * 2, 3) / 2; // Scale to [0, 1] for t in [0.5, 1]

        }
    }

    public struct ScrapNotify : IComponentData
    {
        public float ScrapValue;
    }
}
