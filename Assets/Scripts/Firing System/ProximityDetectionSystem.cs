using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public partial class ProximityDetectionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Create and run the proximity detection job
        var job = new ProximityDetectionJob
        {
            EnemyEntities = GetEntityQuery(ComponentType.ReadOnly<NewEnemySpawn>(), ComponentType.ReadOnly<LocalToWorld>()).ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
        };

        Dependency = job.ScheduleParallel(Dependency);
    }

    // Job to handle proximity detection
    [WithAll(typeof(ProximityComponent))]
    private partial struct ProximityDetectionJob : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalToWorld> EnemyEntities;

        public void Execute(ref ProximityComponent proximity, ref TargetComponent target, in LocalToWorld weaponPosition)
        {
            bool enemyDetected = false;
            float3 closestEnemyPosition = float3.zero;
            float closestDistance = float.MaxValue;

            // Iterate over all enemy entities
            foreach (var enemyPosition in EnemyEntities)
            {
                float distance = math.distance(weaponPosition.Position, enemyPosition.Position);
                if (distance < proximity.ProximityRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemyPosition = enemyPosition.Position;
                    enemyDetected = true;
                }
            }

            proximity.EnemyInProximity = enemyDetected;
            target.HasTarget = enemyDetected;
            if (enemyDetected)
            {
                target.TargetPosition = closestEnemyPosition;
            }
        }
    }
}