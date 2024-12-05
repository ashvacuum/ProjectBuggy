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
            Debug.Log($"New Level Detected {current} {newLevel} {TotalExperience}");
            return current != newLevel;
        }

        public float GetExpPercentToNextUpgrade()
        {/*
            var expRequiredToNextLevel = 0;
            var percentExp = 0f;
            for (var i = 1; i < 1000; i++)
            {
                var requiredToNextLevel = 100 + (10 * (i + 1));
                expRequiredToNextLevel += requiredToNextLevel;
                if (!(expRequiredToNextLevel >= TotalExperience)) continue;
                
                var currentLevelExp = expRequiredToNextLevel - TotalExperience;
                Debug.Log($"Percent Exp {currentLevelExp}/{requiredToNextLevel}");
                percentExp = math.clamp(1 - (currentLevelExp / requiredToNextLevel),0,1) ;
                break;

            }
*/
            var currentLevel = GetCurrentLevel();
            var totalAccumulatedExpDuringLevel = 0;
            for (var i = 0; i < currentLevel; i++)
            {
                totalAccumulatedExpDuringLevel += 100 + (10 * i);
            }

            var expToNextLevel = 100 + 10 * (currentLevel - 1);
            var computedExpAmountDeducted = totalAccumulatedExpDuringLevel - TotalExperience;
            //Debug.Log($"{computedExpAmountDeducted}/{expToNextLevel} {TotalExperience} - {totalAccumulatedExpDuringLevel} {currentLevel}");
            return computedExpAmountDeducted/expToNextLevel;
        }

        public int GetCurrentLevel()
        {
            var level = 0;
            var expRequiredToNextLevel = 100;
            for (var i = 1; i < 1000; i++)
            {
                var levelRequirement = 100 + (10 * i);
                if (TotalExperience >= expRequiredToNextLevel)
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
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            if (!hasExpContainer) return;
           
            foreach (var (experienceBuffers, levelUp, entity) in SystemAPI.Query<DynamicBuffer<ExperienceBuffer>, DynamicBuffer<LevelUpBuffer>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                foreach (var buffer in experienceBuffers)
                {
                    Debug.Log($"Adding Exp: {buffer.Experience}");
                    if (expContainer.ValueRW.AddExperience(buffer.Experience))
                    {
                        levelUp.Add(new LevelUpBuffer());
                        //ecb.AppendToBuffer(entity, new LevelUpBuffer());
                    }
                }
                
                experienceBuffers.Clear();
                    
            }
        }

    }
}
