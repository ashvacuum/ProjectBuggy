using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Entities
{
    public partial class ShipSpawner : SystemBase
    {
        protected override void OnCreate()
        {
            EntityArchetype entityArcheType = EntityManager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld));
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
