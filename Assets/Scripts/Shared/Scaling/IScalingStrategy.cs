using DungeonMaster.Shared.Scaling;
using UnityEngine;

namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 스케일링 전략 인터페이스
    /// Strategy 패턴을 통해 각 스케일링 방식을 분리
    /// </summary>
    public interface IScalingStrategy
    {
        /// <summary>
        /// 스케일링 값 계산
        /// </summary>
        /// <param name="level">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        /// <param name="config">스케일링 설정</param>
        /// <returns>계산된 스케일링 값</returns>
        float Calculate(int level, int maxLevel, ScalingConfig config);

        /// <summary>
        /// 설정 유효성 검증
        /// </summary>
        /// <param name="config">검증할 설정</param>
        /// <returns>검증 결과</returns>
        bool ValidateConfig(ScalingConfig config);

        /// <summary>
        /// 기본 설정 제공
        /// </summary>
        /// <returns>기본 스케일링 설정</returns>
        ScalingConfig GetDefaultConfig();
    }
} 