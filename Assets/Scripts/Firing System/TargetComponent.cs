using Unity.Entities;
using Unity.Mathematics;

public struct TargetComponent : IComponentData
{
    public float3 TargetPosition;
    public bool HasTarget; // Whether the weapon has a target
}