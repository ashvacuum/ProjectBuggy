using Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    /// <summary>
    /// Arcade heading-committed vehicle movement (Phase 1.2 + 1.3 drift — replaces twin-stick
    /// CharacterMovementSystem, the ship->buggy fork point). Throttle (move.y) builds momentum along
    /// the nose; steering (move.x) yaws the nose. Drift is emergent, not a button: a lateral traction
    /// limit (GripAccel) cancels only so much sideways speed per second, so a hard turn at speed
    /// breaks the tail loose and it slides, then regrips. Owns XZ + yaw only; BuggyTerrainSystem owns
    /// Y (suspension), so velocity has no vertical term yet (air is slice 1.4).
    /// </summary>
    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateAfter(typeof(InputSystem))]
    public partial struct VehicleMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VehiclePhysicsData>();
            state.RequireForUpdate<InputsData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            foreach (var (vehicle, inputs, transform) in
                     SystemAPI.Query<RefRW<VehiclePhysicsData>, RefRO<InputsData>, RefRW<LocalTransform>>()
                         .WithAll<PlayerTag>())
            {
                float throttle = inputs.ValueRO.move.y; // W/S
                float steer    = inputs.ValueRO.move.x;  // A/D

                ref var v = ref vehicle.ValueRW;

                // Nitrous: a tap surges Nitro to 1, then it decays over NitroDuration. While active it
                // scales up both top speed and acceleration — a slingshot that fades.
                if (inputs.ValueRO.nitro) v.Nitro = 1f;
                v.Nitro = math.max(0f, v.Nitro - dt / math.max(v.NitroDuration, 0.0001f));

                // Effective top speed folds in the session movement-speed upgrade and the live nitro boost.
                float topSpeed = v.TopSpeed * (1f + v.SpeedBonus * 0.01f) * (1f + v.NitroTopSpeedBonus * 0.01f * v.Nitro);
                float accel    = v.Acceleration * (1f + v.NitroAccelBonus * 0.01f * v.Nitro);

                // Steer the nose, scaled by speed so a parked buggy can't skid-steer in place.
                float speedFactor = math.saturate(math.length(v.Velocity) / math.max(topSpeed, 0.001f));
                float yaw = steer * v.TurnSpeed * dt * speedFactor;
                var rot = math.mul(transform.ValueRO.Rotation, quaternion.RotateY(yaw));
                float3 forward = math.mul(rot, new float3(0f, 0f, 1f)); // planar: RotateY keeps y=0

                // Throttle accelerates along the nose; coast down via friction off-throttle.
                v.Velocity += forward * (throttle * accel * dt);
                if (throttle == 0f)
                    v.Velocity = MoveTowardZero(v.Velocity, v.Friction * dt);

                // Mathematical drift: grip is a lateral traction LIMIT, not a button. It can cancel
                // at most GripAccel of sideways speed per second; whip the nose around fast enough at
                // speed and the tail breaks loose and slides, then regrips as the slide settles.
                float vForward    = math.dot(v.Velocity, forward);
                float3 forwardVel = forward * vForward;
                float3 lateralVel = v.Velocity - forwardVel;
                float lat         = math.length(lateralVel);
                if (lat > 1e-5f)
                    lateralVel *= math.max(0f, lat - v.GripAccel * dt) / lat;
                v.Velocity = forwardVel + lateralVel;

                // Clamp planar speed (separate forward / reverse caps).
                float speed = math.length(v.Velocity);
                float cap   = math.dot(v.Velocity, forward) >= 0f ? topSpeed : v.ReverseSpeed;
                if (speed > cap) v.Velocity *= cap / speed;

                v.CurrentSpeed = math.dot(v.Velocity, forward); // signed forward speed (FOV/UI)

                transform.ValueRW.Rotation = rot;
                float3 pos = transform.ValueRO.Position + v.Velocity * dt; // Y owned by BuggyTerrainSystem
                transform.ValueRW.Position = pos;
            }
        }

        static float3 MoveTowardZero(float3 v, float maxDelta)
        {
            float m = math.length(v);
            return m <= maxDelta ? float3.zero : v - math.normalizesafe(v) * maxDelta;
        }
    }

    /// Arcade vehicle state + config. ponytail: only the fields slices 1.2/1.3 use; IsGrounded /
    /// AngularVelocity / stun land with their slices (1.4, 2.x).
    public struct VehiclePhysicsData : IComponentData
    {
        public float3 Velocity;      // planar world-space momentum (Y owned by suspension)
        public float CurrentSpeed;   // signed forward speed, derived each frame (FOV/UI)
        public float Nitro;          // current nitro charge 0..1 (decays); scales top speed + accel
        public float SpeedBonus;     // session movement-speed upgrade: % added to TopSpeed (starts 0)
        // config (Editor-tuned; per-archetype profiles post-MVP)
        public float TopSpeed;
        public float ReverseSpeed;
        public float Acceleration;
        public float Friction;       // coast-down rate when off throttle (units/s^2)
        public float TurnSpeed;      // radians/sec at full speed
        public float GripAccel;      // lateral traction limit (units/s^2): higher = harder to drift
        public float RideSmoothing;  // Y catch-up rate to terrain (suspension-lite; 0 = hard snap)
        public float NitroTopSpeedBonus; // % extra top speed at full nitro
        public float NitroAccelBonus;    // % extra acceleration at full nitro
        public float NitroDuration;      // seconds for a nitro burst to decay 1 -> 0
    }
}
