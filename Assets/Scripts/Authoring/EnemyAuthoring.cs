using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        public float speed = 20f;
        public float rotationSpeed = 20f;
        public float damage = 1;
        public float targetRangeBounds = 50f;
        private class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NewEnemySpawn());
                AddComponent(entity, new EnemyFollowTarget()
                {
                    Speed = authoring.speed,
                    RotationSpeed = authoring.rotationSpeed,
                    FollowTargetLimits = authoring.targetRangeBounds
                });
                AddComponent(entity, new DamageComponent()
                {
                    Damage = authoring.damage
                });
            }
        }
    }

    public struct EnemyFollowTarget : IComponentData
    {
        public float Speed;
        public float RotationSpeed;
        public float FollowTargetLimits;
    }

    public struct NewEnemySpawn : IComponentData { }
}
