using DungeonMaster.Shared.Scaling;
using UnityEngine;

namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 개별 스케일링 설정 (Strategy 패턴 적용)
    /// 각 요소별 독립적 스케일링을 Strategy 패턴으로 처리
    /// </summary>
    [System.Serializable]
    public struct IndividualScaling
    {
        [Header("스케일링 방식")]
        public ScalingType scalingType;
        
        [Header("스케일링 설정")]
        public ScalingConfig config;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public IndividualScaling(ScalingType type, float perLevel = 0f)
        {
            scalingType = type;
            config = new ScalingConfig(perLevel);
        }

        /// <summary>
        /// 상세 설정 생성자
        /// </summary>
        public IndividualScaling(ScalingType type, ScalingConfig scalingConfig)
        {
            scalingType = type;
            config = scalingConfig;
        }

        /// <summary>
        /// 스케일링 값 계산 (Strategy 패턴 적용)
        /// </summary>
        public float CalculateScaling(int level, int maxLevel)
        {
            return ScalingStrategyFactory.Calculate(scalingType, level, maxLevel, config);
        }

        /// <summary>
        /// 설정 유효성 검증
        /// </summary>
        public bool ValidateConfig()
        {
            return ScalingStrategyFactory.ValidateConfig(scalingType, config);
        }

        /// <summary>
        /// 기본 설정으로 초기화
        /// </summary>
        public void ResetToDefault()
        {
            config = ScalingStrategyFactory.GetDefaultConfig(scalingType);
        }

        /// <summary>
        /// 스케일링 타입 설명 반환
        /// </summary>
        public string GetDescription()
        {
            return ScalingStrategyFactory.GetDescription(scalingType);
        }

        // ===== 기존 호환성을 위한 프로퍼티들 =====
        
        /// <summary>
        /// 레벨당 증가값 (호환성)
        /// </summary>
        public float PerLevelValue
        {
            get => config.perLevelValue;
            set => config.perLevelValue = value;
        }

        /// <summary>
        /// 지수 기준값 (호환성)
        /// </summary>
        public float ExponentialBase
        {
            get => config.exponentialBase;
            set => config.exponentialBase = value;
        }

        /// <summary>
        /// 로그 기준값 (호환성)
        /// </summary>
        public float LogarithmicBase
        {
            get => config.logarithmicBase;
            set => config.logarithmicBase = value;
        }

        /// <summary>
        /// 단계별 레벨들 (호환성)
        /// </summary>
        public int[] StepLevels
        {
            get => config.stepLevels;
            set => config.stepLevels = value;
        }

        /// <summary>
        /// 단계별 증가값들 (호환성)
        /// </summary>
        public float[] StepValues
        {
            get => config.stepValues;
            set => config.stepValues = value;
        }

        /// <summary>
        /// 커스텀 배수들 (호환성)
        /// </summary>
        public float[] CustomMultipliers
        {
            get => config.customMultipliers;
            set => config.customMultipliers = value;
        }
    }
} 