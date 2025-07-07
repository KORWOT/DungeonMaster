using UnityEngine;
using System;
using System.Collections.Generic;

namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 스케일링 없음 전략
    /// </summary>
    public class NoneScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            return 0f;
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            return true; // 항상 유효
        }

        public ScalingConfig GetDefaultConfig()
        {
            return ScalingConfig.Default;
        }
    }

    /// <summary>
    /// 선형 스케일링 전략
    /// </summary>
    public class LinearScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            float levelDiff = level - 1; // 레벨 1이 기준
            return config.perLevelValue * levelDiff;
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            return !float.IsNaN(config.perLevelValue) && !float.IsInfinity(config.perLevelValue);
        }

        public ScalingConfig GetDefaultConfig()
        {
            return new ScalingConfig(0.1f);
        }
    }

    /// <summary>
    /// 지수 스케일링 전략
    /// </summary>
    public class ExponentialScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            float levelDiff = level - 1; // 레벨 1이 기준
            if (levelDiff <= 0) return 0f;
            
            return config.perLevelValue * (Mathf.Pow(config.exponentialBase, levelDiff) - 1f);
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            return config.exponentialBase > 1f && 
                   !float.IsNaN(config.perLevelValue) && 
                   !float.IsInfinity(config.perLevelValue);
        }

        public ScalingConfig GetDefaultConfig()
        {
            return new ScalingConfig(0.1f, 1.1f);
        }
    }

    /// <summary>
    /// 로그 스케일링 전략
    /// </summary>
    public class LogarithmicScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            float levelDiff = level - 1; // 레벨 1이 기준
            if (levelDiff <= 0) return 0f;
            
            return config.perLevelValue * Mathf.Log(1 + levelDiff, config.logarithmicBase);
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            return config.logarithmicBase > 1.1f && 
                   !float.IsNaN(config.perLevelValue) && 
                   !float.IsInfinity(config.perLevelValue);
        }

        public ScalingConfig GetDefaultConfig()
        {
            return new ScalingConfig(0.2f, 1.1f, 2.0f);
        }
    }

    /// <summary>
    /// 단계별 스케일링 전략
    /// </summary>
    public class StepScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            if (config.stepLevels == null || config.stepValues == null) 
                return 0f;
            
            float totalValue = 0f;
            for (int i = 0; i < config.stepLevels.Length && i < config.stepValues.Length; i++)
            {
                if (level >= config.stepLevels[i])
                    totalValue += config.stepValues[i];
            }
            return totalValue;
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            if (config.stepLevels == null || config.stepValues == null)
                return false;
                
            if (config.stepLevels.Length != config.stepValues.Length)
                return false;
                
            // 레벨이 오름차순인지 확인
            for (int i = 1; i < config.stepLevels.Length; i++)
            {
                if (config.stepLevels[i] <= config.stepLevels[i - 1])
                    return false;
            }
            
            return true;
        }

        public ScalingConfig GetDefaultConfig()
        {
            var config = ScalingConfig.Default;
            config.stepLevels = new int[] { 3, 6, 9 };
            config.stepValues = new float[] { 0.5f, 1.0f, 1.5f };
            return config;
        }
    }

    /// <summary>
    /// 커스텀 스케일링 전략
    /// </summary>
    public class CustomScalingStrategy : IScalingStrategy
    {
        public float Calculate(int level, int maxLevel, ScalingConfig config)
        {
            if (config.customMultipliers == null || level <= 1) 
                return 0f;
            
            int index = Mathf.Clamp(level - 1, 0, config.customMultipliers.Length - 1);
            return config.perLevelValue * (config.customMultipliers[index] - 1f);
        }

        public bool ValidateConfig(ScalingConfig config)
        {
            if (config.customMultipliers == null) 
                return false;
                
            // 모든 배수가 유효한 값인지 확인
            foreach (float multiplier in config.customMultipliers)
            {
                if (float.IsNaN(multiplier) || float.IsInfinity(multiplier))
                    return false;
            }
            
            return true;
        }

        public ScalingConfig GetDefaultConfig()
        {
            var config = new ScalingConfig(1.0f);
            config.customMultipliers = new float[] { 1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 
                                                    2.0f, 2.3f, 2.6f, 2.9f, 3.2f };
            return config;
        }
    }
} 