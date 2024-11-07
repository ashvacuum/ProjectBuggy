using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Cinemachine;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct CameraFollowSystem : ISystem
    {
        private EntityQuery entityQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeManagerComponent>();
            state.RequireForUpdate<PlayerTag>();
            entityQuery = SystemAPI.QueryBuilder().WithAll<CameraFollow>().Build();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            if (Camera.main != null)
            {
                var cameraFollow = entityQuery.GetSingleton<CameraFollow>();


                foreach (var (transform, controller) in
                         SystemAPI.Query<RefRW<LocalTransform>, RefRW<PlayerTag>>())
                {
                    var cameraTransform = Camera.main.transform;

                    if (cameraTransform != null)
                    {
                        cameraTransform.position = math.lerp(cameraTransform.position, cameraFollow.Offset + transform.ValueRO.Position,
                            math.sqrt(SystemAPI.Time.DeltaTime * cameraFollow.CameraSpeed));
                        
                        
                        //TODO: find a way to rotate camera and set rotation without turning endlessly, use camera pitch somehow
                    }
                }
                

            }
        }

        
    }

    public struct CameraFollow : IComponentData
    {
        public float3 Offset;
        public float CameraPitch;
        public float CameraSpeed;
    }}
