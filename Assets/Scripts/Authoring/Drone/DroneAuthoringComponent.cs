using ShipECS.Systems;
using Unity.Entities;
using UnityEngine;

public class DroneAuthoringComponent : MonoBehaviour
{
    private class DroneAuthoringComponentBaker : Baker<DroneAuthoringComponent>
    {
        public override void Bake(DroneAuthoringComponent authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            AddComponent<DroneComponent>(entity);
        }
    }
}
