using Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShipECS.Systems.Artillery
{

    [UpdateInGroup(typeof(PausableSystemGroup))]
    [UpdateBefore(typeof(ArtilleryTargetingAndFiringSystem))]
    partial struct ArtilleryQueueSystem : ISystem
    {
        //Goals create a buffer list of targets 
        private float _queueDelay;
        private float _currentQueueDelay;
        private int _artilleryLeftToFire;
        private EntityQuery _playerQuery;

        public void OnCreate(ref SystemState state)
        {
            _queueDelay = .1f;
            _currentQueueDelay = 0f;
            _artilleryLeftToFire = 0;
            _playerQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<PlayerTag>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hasBuffer = SystemAPI.TryGetSingletonBuffer<ArtilleryQueue>(out var artilleryQueue);
            foreach (var (artilleryRef, bonus, transform, targets) in
                     SystemAPI.Query<RefRW<ArtilleryAttack>, RefRO<WeaponBonusStat>, RefRW<LocalTransform>, DynamicBuffer<ArtilleryTarget>>())
            {
                var artillery = new ArtilleryFiringAspect(artilleryRef, bonus, transform, targets);
                if (artillery.CurrentFireRate > 0)
                {
                    artillery.CurrentFireRate -= SystemAPI.Time.DeltaTime;
                    continue;
                }
                
                _artilleryLeftToFire += artillery.TotalCount;
                artillery.CurrentFireRate = artillery.TotalFireRate;
            }

            if (_currentQueueDelay >= _queueDelay)
            {
                _currentQueueDelay = 0;
                if (_artilleryLeftToFire > 0 && hasBuffer)
                {
                    _artilleryLeftToFire = math.max(0, _artilleryLeftToFire - 1);  //prevents artillery from going below 0
                    artilleryQueue.Add(new ArtilleryQueue());

                }
            }
            
            _currentQueueDelay += SystemAPI.Time.DeltaTime;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
    }
    
    //an item in this existing means that there is an artillery item that should be fired by the firing system
    public struct ArtilleryQueue : IBufferElementData { }
    
    public struct ArtilleryAttack  : IComponentData
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
    
    public readonly struct ArtilleryFiringAspect
    {
        private readonly RefRW<ArtilleryAttack> _artillery;
        private readonly RefRO<WeaponBonusStat> _bonusStats;
        private readonly RefRW<LocalTransform> _transform;
        private readonly DynamicBuffer<ArtilleryTarget> _targets;

        public ArtilleryFiringAspect(RefRW<ArtilleryAttack> artillery, RefRO<WeaponBonusStat> bonusStats,
            RefRW<LocalTransform> transform, DynamicBuffer<ArtilleryTarget> targets)
        {
            _artillery = artillery;
            _bonusStats = bonusStats;
            _transform = transform;
            _targets = targets;
        }

        public float TotalFireRate => math.max(0,
            _artillery.ValueRW.BaseFireRate -
            _bonusStats.ValueRO.FireRateReductionBonus / 100f * _artillery.ValueRW.BaseFireRate);
        
        public int TotalCount => _artillery.ValueRO.BaseNumProjectile + _bonusStats.ValueRO.NumCountBonus;
        public int TotalPenetration => math.max(1,_artillery.ValueRO.BasePenetration + _bonusStats.ValueRO.PenetrationBonus);
        public float TotalDamage => _artillery.ValueRO.BaseDamage + (_bonusStats.ValueRO.DamageBonus/100f * _artillery.ValueRO.BaseDamage);
        public float TotalRange => _artillery.ValueRO.BaseRange + (_bonusStats.ValueRO.RangeBonus/100f * _artillery.ValueRO.BaseRange);
        public float TotalSize =>  _artillery.ValueRO.BaseSize + (_bonusStats.ValueRO.SizeBonus/100f * _artillery.ValueRO.BaseSize);
        public float TotalLifeTime => _artillery.ValueRO.BaseLifeTime + (_bonusStats.ValueRO.LifetimeBonus/100 * _artillery.ValueRO.BaseLifeTime);
        public float TotalSpeed => _artillery.ValueRO.BaseSpeed; // movement-speed bonus moved to VehiclePhysicsData
        public float TotalKnockback => _artillery.ValueRO.BaseKnockback - _bonusStats.ValueRO.KnockbackBonus/100f * _artillery.ValueRO.BaseKnockback;
        public float3 Position => _transform.ValueRO.Position;
        public float3 Forward => _transform.ValueRO.Forward();
        public float3 Direction => Forward - Position;
        public float TotalCritical => _artillery.ValueRO.BaseCritical + (_bonusStats.ValueRO.CriticalBonus / 100f * _artillery.ValueRO.BaseCritical);
        
        public float CurrentFireRate
        {
            get => _artillery.ValueRO.CurrentFireRate;
            set => _artillery.ValueRW.CurrentFireRate = value;
        }

        public float3 GetPosition(ref int number)
        {
            var newPos = _targets[number].TargetLocation;
            number += 1;
            if (number >= _targets.Length)
            {
                number = 0;
            }
            return newPos;
        }
        
        public void CalculatePositions()
        {
            var buffer = _targets;
            buffer.Clear();
        
            float3 direction = Forward - Position;
            float radius = math.min(TotalRange, 700) ;
            float forwardAngle = math.atan2(direction.z, direction.x) * math.TODEGREES;
            float angleIncrement = 2f * math.PI / TotalPenetration;
        
            for (int i = 0; i < TotalPenetration; i++)
            {
                float angle = forwardAngle + i * angleIncrement;
                float3 position = new float3(
                    Position.x + radius * math.cos(angle),
                    Position.y,
                    Position.z + radius * math.sin(angle)
                );
            
                buffer.Add(new ArtilleryTarget { TargetLocation = position });
            }
        }
        

    }
}
