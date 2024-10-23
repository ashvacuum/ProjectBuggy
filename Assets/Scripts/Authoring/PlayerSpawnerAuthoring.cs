using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;

        class Baker : Baker<PlayerSpawnerAuthoring>
        {
            public override void Bake(PlayerSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerSpawner()
                {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
                });
                
            }
        }
    }

    struct PlayerSpawner : IComponentData
    {
        public Entity Prefab;
    }
}
