using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CharacterMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var( data, inputs, transform) in
                 SystemAPI.Query<RefRO<CharacterData>, RefRO<InputsData>, RefRW<LocalTransform>>())
        {
            float3 position = transform.ValueRO.Position;
            
            float3 moveDirection = new float3(inputs.ValueRO.move.x, 0, inputs.ValueRO.move.y); // Assuming y is up
            position += moveDirection * data.ValueRO.speed * SystemAPI.Time.DeltaTime;
            
            transform.ValueRW.Position = position;
            
          
            
            if (math.length(moveDirection) > 0.01f) // Only rotate if there's significant movement
            {
                quaternion rotation = quaternion.LookRotationSafe(math.normalize(moveDirection), math.up()); // Use the up vector for rotation
                transform.ValueRW.Rotation = rotation;
            }
        }
    }
}
