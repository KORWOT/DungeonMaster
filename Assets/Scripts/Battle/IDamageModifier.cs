using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 피해 계산 수정자 인터페이스 (확장 가능한 피해 계산 로직)
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>
        /// 피해량을 수정하는 메서드
        /// </summary>
        /// <param name="damage">원본 피해량</param>
        /// <param name="attacker">공격자 데이터</param>
        /// <param name="defender">방어자 데이터</param>
        /// <param name="context">추가 컨텍스트 (스킬 정보 등)</param>
        /// <returns>수정된 피해량</returns>
        long ModifyDamage(long damage, DeterministicCharacterData attacker, DeterministicCharacterData defender, DamageContext context);
        
        /// <summary>
        /// 수정자 우선순위 (낮을수록 먼저 적용)
        /// </summary>
        int Priority { get; }
    }
    
    /// <summary>
    /// 피해 계산 컨텍스트 (추가 정보 전달용)
    /// </summary>
    public class DamageContext
    {
        public long SkillMultiplier { get; set; } = 100; // 스킬 배율 (100 = 1.0배)
        public bool IsCritical { get; set; } = false; // 치명타 여부
        public long AttributeMultiplier { get; set; } = 100; // 속성 상성 배율 (100 = 1.0배)
        public int HitCount { get; set; } = 1; // 히트 수
        public string SkillName { get; set; } = ""; // 스킬 이름
        
        // 나중에 추가될 컨텍스트 정보들
        // 예: 특수 효과, 환경 요소 등
    }
}