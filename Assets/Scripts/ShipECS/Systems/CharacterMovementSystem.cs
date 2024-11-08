using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    [UpdateInGroup(typeof(PausableSystemGroup))]
    public partial struct CharacterMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CharacterData>();
            state.RequireForUpdate<InputsData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var( data, inputs, transform) in
                     SystemAPI.Query<RefRO<CharacterData>, RefRO<InputsData>, RefRW<LocalTransform>>().WithAll<PlayerTag>())
            {
                
                var position = transform.ValueRO.Position;
            
                var moveDirection = new float3(inputs.ValueRO.move.x, 0, inputs.ValueRO.move.y); // Assuming y is up
                
                if(math.length(moveDirection) <= .05f) continue;
                position += moveDirection * data.ValueRO.moveSpeed * SystemAPI.Time.DeltaTime;
            
                transform.ValueRW.Position = new float3(position.x, 0, position.z);


                if (!(math.length(moveDirection) > 0.01f)) continue; // Only rotate if there's significant movement
                var rotation = quaternion.LookRotationSafe(math.normalize(moveDirection), math.up()); // Use the up vector for rotation
                //calculate lerped rotation
                var lerpedRot = math.slerp(transform.ValueRO.Rotation,rotation,SystemAPI.Time.DeltaTime * data.ValueRO.rotSpeed);
                transform.ValueRW.Rotation = lerpedRot;
            }
        }
    }
}
