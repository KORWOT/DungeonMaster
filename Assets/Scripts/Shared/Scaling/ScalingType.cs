namespace DungeonMaster.Shared.Scaling
{
    /// <summary>
    /// 값의 레벨 스케일링 방식을 정의합니다.
    /// </summary>
    public enum ScalingType
    {
        None,         // 스케일링 없음
        Linear,       // 선형 증가 (레벨당 일정량)
        Exponential,  // 지수 증가 (1.1^level 형태)
        Logarithmic,  // 로그 증가 (초반 빠름, 후반 느림)
        Step,         // 단계별 증가 (특정 레벨에서만)
        Custom        // 커스텀 배수 테이블
    }
} 