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
    [UpdateAfter(typeof(ProjectBuggy.BuggyTerrainSystem))] // read the player's finalized Y, not last frame's
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
                        float dt = SystemAPI.Time.DeltaTime;
                        float3 cam = cameraTransform.position;
                        float3 target = cameraFollow.Offset + transform.ValueRO.Position;

                        // Vertical lag: Y tracks slower than XZ so a launch climbs the screen frame
                        // before the camera catches up. ponytail: fall back to CameraSpeed when
                        // VerticalSpeed is unset (0) so older baked prefabs don't freeze the camera Y.
                        float vSpeed = cameraFollow.VerticalSpeed > 0f ? cameraFollow.VerticalSpeed : cameraFollow.CameraSpeed;
                        float tXZ = math.sqrt(dt * cameraFollow.CameraSpeed);
                        float tY  = math.sqrt(dt * vSpeed);

                        cam.x = math.lerp(cam.x, target.x, tXZ);
                        cam.z = math.lerp(cam.z, target.z, tXZ);
                        cam.y = math.lerp(cam.y, target.y, tY);
                        cameraTransform.position = cam;

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
        public float VerticalSpeed; // 0 = use CameraSpeed (no lag)
    }}
