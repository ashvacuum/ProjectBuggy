using Authoring;
using Authoring.Drone;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShipECS.Systems
{
    public partial struct DroneSpawningSystem : ISystem
    {

        private EntityQuery _droneComponentQuery;
        public void OnCreate(ref SystemState state)
        {
            _droneComponentQuery = state.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly(typeof(DroneComponent)),
                ComponentType.ReadOnly(typeof(LocalTransform)));
            
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var hasDrone = SystemAPI.TryGetSingletonBuffer<DroneDatabase>(out var droneDatabases);
            foreach (var (droneRef, bonus, transform) in
                     SystemAPI.Query<RefRW<DroneAttack>, RefRO<PlayerBonusStat>, RefRW<LocalTransform>>())
            {
                var artillery = new DroneAspect(droneRef, bonus, transform);

                var remainingDronesToSpawn = 0;
                if (_droneComponentQuery.IsEmpty)
                {
                    remainingDronesToSpawn = artillery.TotalCount;
                }
                else
                {
                    remainingDronesToSpawn = artillery.TotalCount - _droneComponentQuery.ToEntityArray(Allocator.Temp).Length;
                }

                if (!hasDrone) continue;
                
                if (droneDatabases.Length <= 0) continue;
                
                for (var i = 0; i < remainingDronesToSpawn; i++)
                {
                    ecb.Instantiate(droneDatabases[0].PrefabEntity);
                }

            }
        }
    }

    
    /// <summary>
    /// Refers to actual drones that can turn and fire
    /// </summary>
    public struct DroneComponent : IComponentData
    {
        
    }

    public readonly struct DroneAspect
    {
        private readonly RefRW<DroneAttack> _droneAttack;
        private readonly RefRO<PlayerBonusStat> _bonusStats;
        private readonly RefRW<LocalTransform> _transform;

        public DroneAspect(RefRW<DroneAttack> droneAttack, RefRO<PlayerBonusStat> bonusStats,
            RefRW<LocalTransform> transform)
        {
            _droneAttack = droneAttack;
            _bonusStats = bonusStats;
            _transform = transform;
        }

        public float TotalFireRate => math.max(0,
            _droneAttack.ValueRW.BaseFireRate -
            _bonusStats.ValueRO.FireRateReductionBonus / 100f * _droneAttack.ValueRW.BaseFireRate);
        
        public int TotalCount => _droneAttack.ValueRO.BaseNumProjectile + _bonusStats.ValueRO.NumCountBonus;
        public int TotalPenetration => math.max(1,_droneAttack.ValueRO.BasePenetration + _bonusStats.ValueRO.PenetrationBonus);
        public float TotalDamage => _droneAttack.ValueRO.BaseDamage + (_bonusStats.ValueRO.DamageBonus/100f * _droneAttack.ValueRO.BaseDamage);
        public float TotalRange => _droneAttack.ValueRO.BaseRange + (_bonusStats.ValueRO.RangeBonus/100f * _droneAttack.ValueRO.BaseRange);
        public float TotalSize =>  _droneAttack.ValueRO.BaseSize + (_bonusStats.ValueRO.SizeBonus/100f * _droneAttack.ValueRO.BaseSize);
        public float TotalLifeTime => _droneAttack.ValueRO.BaseLifeTime + (_bonusStats.ValueRO.LifetimeBonus/100 * _droneAttack.ValueRO.BaseLifeTime);
        public float TotalSpeed => _droneAttack.ValueRO.BaseSpeed + _bonusStats.ValueRO.SpeedBonus/100f * _droneAttack.ValueRO.BaseSpeed;
        public float TotalKnockback => _droneAttack.ValueRO.BaseKnockback - _bonusStats.ValueRO.KnockbackBonus/100f * _droneAttack.ValueRO.BaseKnockback;
        public float3 Position => _transform.ValueRO.Position;
        public float3 Forward => _transform.ValueRO.Forward();
        public float3 Direction => Forward - Position;
        public float TotalCritical => _droneAttack.ValueRO.BaseCritical + (_bonusStats.ValueRO.CriticalBonus / 100f * _droneAttack.ValueRO.BaseCritical);
        
        public float CurrentFireRate
        {
            get => _droneAttack.ValueRO.CurrentFireRate;
            set => _droneAttack.ValueRW.CurrentFireRate = value;
        }

    }
    
    public struct DroneAttack  : IComponentData
    {
        public float BaseFireRate;
        public int BaseNumProjectile;
        public int BasePenetration;
        public float BaseDamage;
        public float BaseRange;
        public float BaseSize;
        public float BaseLifeTime;
        public float BaseSpeed;
        public float BaseKnockback;
        public float BaseCritical;
        public float CurrentFireRate; // value to edit if it hits 0 it will fire and reset to total fire rate
    }
}
