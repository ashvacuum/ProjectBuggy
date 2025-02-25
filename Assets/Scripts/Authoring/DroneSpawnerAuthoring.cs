using ShipECS.Systems;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class DroneSpawnerAuthoring : MonoBehaviour
    {
        public GameObject droneEntity;

        private class DroneSpawnerBaker : Baker<DroneSpawnerAuthoring>
        {
            public override void Bake(DroneSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<DroneDatabase>(entity);
                buffer.Add(new DroneDatabase()
                {
                    PrefabEntity = GetEntity(authoring.droneEntity, TransformUsageFlags.Dynamic)
                });
                AddComponent(entity, new DroneComponent());
            }


        }
    }

    public struct DroneDatabase : IBufferElementData
    {
        public Entity PrefabEntity;
    }
    
}


