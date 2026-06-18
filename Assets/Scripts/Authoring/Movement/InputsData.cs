using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct InputsData : IComponentData
{
    public float2 move;
    public bool nitro; // Sprint tapped this frame = trigger a nitro burst
}
