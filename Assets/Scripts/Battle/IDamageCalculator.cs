using DungeonMaster.Data;
using DungeonMaster.Skill;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 데미지 계산 로직을 추상화하는 인터페이스입니다.
    /// </summary>
    public interface IDamageCalculator
    {
        /// <summary>
        /// 데미지 계산 파이프라인을 실행하여 최종 데미지를 계산합니다.
        /// </summary>
        /// <param name="attacker">공격자 데이터</param>
        /// <param name="defender">방어자 데이터</param>
        /// <param name="skill">사용된 스킬 데이터</param>
        /// <returns>계산된 최종 데미지</returns>
        long Calculate(DeterministicCharacterData attacker, DeterministicCharacterData defender, SkillData skill);
    }
} 