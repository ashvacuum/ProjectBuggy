using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        private class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LocalTransform());
                AddComponent(entity, new NewEnemySpawn());
                AddComponent(entity, new EnemyFollowTarget());
            }
        }
    }

    public struct EnemyFollowTarget : IComponentData { }

    public struct NewEnemySpawn : IComponentData { }
}
