using Unity.Entities;

public struct ProximityComponent : IComponentData
{
    public float ProximityRadius;
    public bool EnemyInProximity;
}