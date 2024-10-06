using Unity.Entities;

public struct LaserComponent : IComponentData
{
    public float BarrelOffsetTime; // Delay between the two barrels
    public bool IsFiring;
    public float LastFireTime; // Keeps track of the last time it fired
}