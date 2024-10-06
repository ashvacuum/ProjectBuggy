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
        private class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NewEnemySpawn());
                AddComponent(entity, new EnemyFollowTarget()
                {
                    Speed = authoring.speed,
                    RotationSpeed = authoring.rotationSpeed
                });
                AddComponent(entity, new DamageComponent()
                {
                    Damage = authoring.damage
                });
                
                /*AddComponent(entity, new ()
                {
                    
                });*/
            }
        }
    }

    public struct EnemyFollowTarget : IComponentData
    {
        public float Speed;
        public float RotationSpeed;
    }

    public struct NewEnemySpawn : IComponentData { }
}
