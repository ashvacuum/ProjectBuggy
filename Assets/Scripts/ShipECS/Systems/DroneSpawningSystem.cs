using Authoring;
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
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
    }

    public struct DroneComponent : IComponentData
    {
        
    }

    public readonly partial struct DroneAspect : IAspect
    {
        private readonly RefRW<DroneAttack> _droneAttack;
        private readonly RefRO<PlayerBonusStat> _bonusStats;
        private readonly RefRW<LocalTransform> _transform;


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
