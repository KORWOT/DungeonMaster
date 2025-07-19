using System.Collections.Generic;

namespace DungeonMaster.Data
{
    /// <summary>
    /// 전투에 참여하는 모든 데이터 객체가 구현해야 하는 공통 인터페이스입니다.
    /// dynamic 키워드 사용을 피하고 타입 안전성을 보장하기 위해 정의되었습니다.
    /// </summary>
    public interface ICombatantData
    {
        /// <summary>
        /// 전투 중 이 개체를 식별하는 고유 ID (한 번 설정되면 변경되지 않음)
        /// </summary>
        long InstanceId { get; }
        
        /// <summary>
        /// 현재 활성화된 버프 목록
        /// </summary>
        List<BuffData> ActiveBuffs { get; set; }
        
        /// <summary>
        /// 공격 쿨타임 잔여 시간 (밀리초)
        /// </summary>
        long AttackCooldownRemainingMs { get; set; }
        
        /// <summary>
        /// 스킬별 쿨타임 잔여 시간 (스킬 ID -> 잔여 시간)
        /// </summary>
        Dictionary<long, long> SkillCooldowns { get; set; }
    }
} 