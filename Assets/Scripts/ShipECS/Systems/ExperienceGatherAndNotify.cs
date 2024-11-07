using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShipECS.Systems
{
    public struct ExperienceBuffer : IBufferElementData
    {
        public float Experience;
    }

    public struct LevelUpBuffer : IBufferElementData
    {
    }

    public struct ExperienceContainer : IComponentData
    {
        public float TotalExperience;
        public float BonusExperience;

        public bool AddExperience(float inExp)
        {
            var current = GetCurrentLevel();
            TotalExperience += inExp + (BonusExperience / 100 * inExp);
            var newLevel = GetCurrentLevel();
            return current != newLevel;
        }

        public float GetExpPercentToNextUpgrade()
        {
            var expRequiredToNextLevel = 0f;
            var percentExp = 0f;
            for (var i = 0; i < 1000; i++)
            {
                var levelRequirement = 100 + (10 * (i + 1));
                expRequiredToNextLevel += levelRequirement;
                if (!(expRequiredToNextLevel > TotalExperience)) continue;
                
                var currentLevelExp = expRequiredToNextLevel - TotalExperience;
                percentExp = math.clamp(1 - (currentLevelExp / levelRequirement),0,1) ;
                break;

            }

            return percentExp;
        }

        public int GetCurrentLevel()
        {
            var level = 0;
            var expRequiredToNextLevel = 0f;
            for (var i = 1; i < 1000; i++)
            {
                var levelRequirement = 100 + (10 * i);
                if (TotalExperience > expRequiredToNextLevel)
                {
                    expRequiredToNextLevel += levelRequirement;
                }
                else
                {
                    level = i;
                    break;
                }
            }

            return level;
        }
    }
    [UpdateInGroup(typeof(PausableSystemGroup))]
    partial struct ExperienceGatherAndNotify : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            var hasExpContainer = SystemAPI.TryGetSingletonRW<ExperienceContainer>(out var expContainer);
            if (!hasExpContainer) return;

            foreach (var (experienceBuffers, levelUp) in SystemAPI.Query<DynamicBuffer<ExperienceBuffer>, DynamicBuffer<LevelUpBuffer>>().WithAll<PlayerTag>())
            {
                foreach (var buffer in experienceBuffers)
                {
                    Debug.Log($"Adding Exp: {buffer.Experience}");
                    if (expContainer.ValueRW.AddExperience(buffer.Experience))
                    {
                        
                        levelUp.Add(new LevelUpBuffer());
                    }
                }
                
                experienceBuffers.Clear();
                    
            }
        }

    }
}
