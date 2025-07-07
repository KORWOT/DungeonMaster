using UnityEngine;

namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 스케일링 설정 데이터
    /// 각 전략별로 필요한 설정들을 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct ScalingConfig
    {
        [Header("공통 설정")]
        public float perLevelValue;      // 레벨당 기본 증가값

        [Header("지수/로그 스케일링")]
        public float exponentialBase;    // 지수 기준값 (기본 1.1)
        public float logarithmicBase;    // 로그 기준값 (기본 2.0)

        [Header("단계별 스케일링")]
        public int[] stepLevels;         // 증가가 일어나는 레벨들
        public float[] stepValues;       // 각 단계별 증가값

        [Header("커스텀 스케일링")]
        public float[] customMultipliers; // 레벨별 배수

        /// <summary>
        /// 기본 설정으로 초기화
        /// </summary>
        public static ScalingConfig Default => new ScalingConfig
        {
            perLevelValue = 0f,
            exponentialBase = 1.1f,
            logarithmicBase = 2.0f,
            stepLevels = null,
            stepValues = null,
            customMultipliers = null
        };

        /// <summary>
        /// 지정된 값으로 초기화
        /// </summary>
        public ScalingConfig(float perLevel = 0f, float expBase = 1.1f, float logBase = 2.0f)
        {
            perLevelValue = perLevel;
            exponentialBase = expBase;
            logarithmicBase = logBase;
            stepLevels = null;
            stepValues = null;
            customMultipliers = null;
        }
    }
} 