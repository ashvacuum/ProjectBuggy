using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PhysicsDebugDisplayGroup))]
    public partial struct CapsuleCastSystem : ISystem
    {
        // Gizmo-related fields to store cast information
        private float3 m_StartPoint;
        private float3 m_EndPoint;
        private float m_Radius;
        private bool m_HasHit;
        private NativeArray<ColliderCastHit> m_HitResults;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            // Initialize any necessary setup
            //ensure it will hit store a maximum of 10 collisions
            m_HitResults = new NativeArray<ColliderCastHit>(10, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) 
        {
            // Clean up native array
            m_HitResults.Dispose();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            return;
            // Define capsule cast parameters
            m_StartPoint = new float3(0, 1, 0);  // Starting point of the capsule
            m_EndPoint = new float3(0, 5, 0);    // Endpoint of the capsule
            m_Radius = 0.5f;                     // Radius of the capsule
            float maxDistance = 10f;             // Maximum cast distance

            // Create a CollisionWorld reference
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var collisionWorld = physicsWorldSingleton.CollisionWorld;


            foreach (var transform in
                     SystemAPI.Query<RefRW<LocalTransform>>()
                         .WithAll<PlayerTag>())
            {
                m_StartPoint = transform.ValueRO.Position;
                m_EndPoint = transform.ValueRO.Forward() * 10;
                m_Radius = .1f;
                maxDistance = 20f;
            }

            // Create a NativeList to store results
            var hitResults = new NativeList<ColliderCastHit>(Allocator.Temp);
            
            
            // Perform the capsule cast
            m_HasHit = collisionWorld.CapsuleCastAll(m_StartPoint,
                m_EndPoint, m_Radius, m_EndPoint - m_StartPoint, maxDistance,
                ref hitResults,
                Unity.Physics.CollisionFilter.Default
            );

            // Store hit results for Gizmos
            if (m_HasHit)
            {
                // Copy hit results for Gizmo drawing
                m_HitResults = hitResults.ToArray(Allocator.Temp);

                // Process the results
                foreach (var hit in hitResults)
                {
                    UnityEngine.Debug.Log($"Capsule Cast Hit: " +
                                          $"Point={hit.Position}, " +
                                          $"Distance={hit.Fraction * maxDistance}, " +
                                          $"Entity Index={hit.Entity.Index}");
                }
            }

            // Clean up
            hitResults.Dispose();
        }

        // Custom Gizmo drawing method
        [DrawGizmo(GizmoType.Active | GizmoType.NonSelected)]
        private static void DrawCapsuleCastGizmos(CapsuleCastSystem system, GizmoType gizmoType)
        {
            // Draw the capsule cast path
            Gizmos.color = Color.yellow;
        
            // Draw the capsule start and end points
            Gizmos.DrawWireSphere(system.m_StartPoint, system.m_Radius);
            Gizmos.DrawWireSphere(system.m_EndPoint, system.m_Radius);

            // Draw the capsule path line
            Gizmos.DrawLine(system.m_StartPoint, system.m_EndPoint);

            // If hit detected, draw hit visualization
            if (system.m_HasHit)
            {
                Gizmos.color = Color.red;
            
                // Draw hit points
                foreach (var hit in system.m_HitResults)
                {
                    Gizmos.DrawSphere(hit.Position, system.m_Radius * 1.2f);
                
                    // Optional: Draw hit normal
                    Gizmos.DrawRay(hit.Position, hit.SurfaceNormal * system.m_Radius);
                }
            }
        }
    }

// Companion component (optional, for attaching to an entity if needed)
    public struct CapsuleCastTag : IComponentData { }
}