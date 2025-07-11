using System;
using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Shared.Scaling
{
    [CreateAssetMenu(fileName = "GrowthConfig", menuName = "DungeonMaster/Growth Config", order = 0)]
    public class GrowthConfig : ScriptableObject
    {
        [Header("Experience Requirements")]
        [Tooltip("Array where index = level, value = experience required for that level.")]
        public long[] ExperiencePerLevel;

        [Header("Level Up Cost")]
        [Tooltip("Array where index = level, value = gold cost to level up to that level.")]
        public int[] GoldCostPerLevel;
        
        [Tooltip("Array where index = level, value = gem cost to level up to that level.")]
        public int[] GemCostPerLevel;

        [Header("Grade Growth Settings")]
        [SerializeField] private GradeGrowthConfig _gradeGrowthConfig;

        public long GetExperienceForLevel(int level)
        {
            if (level < 0 || level >= ExperiencePerLevel.Length)
            {
                GameLogger.LogError($"Invalid level requested: {level}");
                return long.MaxValue;
            }
            return ExperiencePerLevel[level];
        }

        public (int gold, int gems) GetCostForLevel(int targetLevel)
        {
            if (targetLevel < 0 || targetLevel >= GoldCostPerLevel.Length || targetLevel >= GemCostPerLevel.Length)
            {
                GameLogger.LogError($"Invalid target level for cost lookup: {targetLevel}");
                return (-1, -1); // Indicating an error
            }
            return (GoldCostPerLevel[targetLevel], GemCostPerLevel[targetLevel]);
        }

        public Dictionary<StatType, int> GetBaseGrowthForGrade(Grade grade, StatType statType)
        {
            if (_gradeGrowthConfig == null)
            {
                GameLogger.LogError("GradeGrowthConfig is not assigned in the GrowthConfig asset.");
                return new Dictionary<StatType, int>();
            }
            
            return _gradeGrowthConfig.GetBaseGrowthForGrade(grade, statType);
        }
    }
} 