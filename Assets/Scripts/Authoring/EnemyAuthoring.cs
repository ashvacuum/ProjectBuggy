using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {

        public float speed = 20f;
        private class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LocalTransform());
                AddComponent(entity, new NewEnemySpawn());
                AddComponent(entity, new EnemyFollowTarget()
                {
                    Speed = authoring.speed
                });
            }
        }
    }

    public struct EnemyFollowTarget : IComponentData
    {
        public float Speed;
    }

    public struct NewEnemySpawn : IComponentData { }
}
