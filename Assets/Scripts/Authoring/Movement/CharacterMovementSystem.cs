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
            position.x += inputs.ValueRO.move.x * data.ValueRO.speed * SystemAPI.Time.DeltaTime;
            position.y += inputs.ValueRO.move.y * data.ValueRO.speed * SystemAPI.Time.DeltaTime;
            transform.ValueRW.Position = position;
        }
    }
}
