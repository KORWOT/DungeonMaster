using System;
using System.Collections.Generic;
using UnityEngine;
using DungeonMaster.Localization;

namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 스케일링 전략 팩토리
    /// ScalingType에 따라 적절한 IScalingStrategy 객체를 생성
    /// </summary>
    public static class ScalingStrategyFactory
    {
        // 전략 객체들을 캐싱하여 성능 최적화
        private static readonly Dictionary<ScalingType, IScalingStrategy> _strategyCache 
            = new Dictionary<ScalingType, IScalingStrategy>();

        /// <summary>
        /// 스케일링 타입에 따른 전략 객체 생성/반환
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <returns>해당하는 스케일링 전략</returns>
        public static IScalingStrategy GetStrategy(ScalingType scalingType)
        {
            // 캐시에서 먼저 확인
            if (_strategyCache.TryGetValue(scalingType, out IScalingStrategy cachedStrategy))
            {
                return cachedStrategy;
            }

            // 새로운 전략 객체 생성
            IScalingStrategy strategy = CreateStrategy(scalingType);
            
            // 캐시에 저장
            _strategyCache[scalingType] = strategy;
            
            return strategy;
        }

        /// <summary>
        /// 새로운 전략 객체 생성
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <returns>생성된 전략 객체</returns>
        private static IScalingStrategy CreateStrategy(ScalingType scalingType)
        {
            switch (scalingType)
            {
                case ScalingType.None:
                    return new NoneScalingStrategy();
                    
                case ScalingType.Linear:
                    return new LinearScalingStrategy();
                    
                case ScalingType.Exponential:
                    return new ExponentialScalingStrategy();
                    
                case ScalingType.Logarithmic:
                    return new LogarithmicScalingStrategy();
                    
                case ScalingType.Step:
                    return new StepScalingStrategy();
                    
                case ScalingType.Custom:
                    return new CustomScalingStrategy();
                    
                default:
                    Debug.LogWarning($"알 수 없는 스케일링 타입: {scalingType}. None 전략을 사용합니다.");
                    return new NoneScalingStrategy();
            }
        }

        /// <summary>
        /// 해당 스케일링 타입의 기본 설정 반환
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <returns>기본 설정</returns>
        public static ScalingConfig GetDefaultConfig(ScalingType scalingType)
        {
            IScalingStrategy strategy = GetStrategy(scalingType);
            return strategy.GetDefaultConfig();
        }

        /// <summary>
        /// 설정 유효성 검증
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <param name="config">검증할 설정</param>
        /// <returns>검증 결과</returns>
        public static bool ValidateConfig(ScalingType scalingType, ScalingConfig config)
        {
            IScalingStrategy strategy = GetStrategy(scalingType);
            return strategy.ValidateConfig(config);
        }

        /// <summary>
        /// 스케일링 값 계산
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="config">스케일링 설정</param>
        /// <returns>계산된 스케일링 값</returns>
        public static float Calculate(ScalingType scalingType, int level, int maxLevel, ScalingConfig config)
        {
            IScalingStrategy strategy = GetStrategy(scalingType);
            return strategy.Calculate(level, maxLevel, config);
        }

        /// <summary>
        /// 지원되는 모든 스케일링 타입 반환
        /// </summary>
        /// <returns>스케일링 타입 배열</returns>
        public static ScalingType[] GetSupportedTypes()
        {
            return (ScalingType[])Enum.GetValues(typeof(ScalingType));
        }

        /// <summary>
        /// 스케일링 타입 설명 반환
        /// </summary>
        /// <param name="scalingType">스케일링 타입</param>
        /// <returns>설명 문자열</returns>
        public static string GetDescription(ScalingType scalingType)
        {
            var lm = LocalizationManager.Instance;
            switch (scalingType)
            {
                case ScalingType.None:
                    return lm.GetText("scaling_desc_none");
                    
                case ScalingType.Linear:
                    return lm.GetText("scaling_desc_linear");
                    
                case ScalingType.Exponential:
                    return lm.GetText("scaling_desc_exponential");
                    
                case ScalingType.Logarithmic:
                    return lm.GetText("scaling_desc_logarithmic");
                    
                case ScalingType.Step:
                    return lm.GetText("scaling_desc_step");
                    
                case ScalingType.Custom:
                    return lm.GetText("scaling_desc_custom");
                    
                default:
                    return lm.GetText("scaling_desc_unknown");
            }
        }

        /// <summary>
        /// 캐시 초기화
        /// 주의: 이 메서드는 테스트나 메모리 절약 시에만 사용하세요
        /// </summary>
        public static void ClearCache()
        {
            _strategyCache.Clear();
        }
    }
} 