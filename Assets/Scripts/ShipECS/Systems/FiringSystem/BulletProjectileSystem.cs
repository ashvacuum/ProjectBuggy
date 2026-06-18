using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;


[DisableAutoCreation] // ponytail: dead parallel firing path; player uses ProjectileAttack
public partial struct BulletProjectileSystem : ISystem
{
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
                state.RequireForUpdate<Projectile>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
                var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

                var bulletProjectibleJob = new BulletProjectileJob
                {
                        ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                        DeltaTime = SystemAPI.Time.DeltaTime
                };

                bulletProjectibleJob.Schedule();
        }
}

[BurstCompile]
public partial struct BulletProjectileJob : IJobEntity
{
        public EntityCommandBuffer ECB;
        public float DeltaTime;
        
        void Execute(Entity entity,ref Projectile projectile, ref LocalTransform transform)
        {
                var gravity = new float3(0f, -9.81f, 0f);
                var invertY = new  float3(1.0f, -1.0f, 1.0f);
                
                transform.Position += projectile.Velocity * DeltaTime;
                
                //bounce
                /*
                if (transform.Position.y < 0f)
                {
                        transform.Position *= invertY;
                        projectile.Velocity *= invertY * 0.8f;
                }
                */
                
                
                //adds gravity
                //projectile.Velocity += gravity * DeltaTime;
                
                //Destory after few bounces
                if (math.lengthsq(projectile.Velocity) < 0.01f)
                {
                        ECB.DestroyEntity(entity);
                }
        }
} 