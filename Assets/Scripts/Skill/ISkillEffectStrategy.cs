using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬의 개별 효과를 실행하는 전략 인터페이스입니다.
    /// </summary>
    public interface ISkillEffectStrategy
    {
        /// <summary>
        /// 스킬 효과를 대상에게 적용합니다.
        /// </summary>
        /// <param name="battleState">현재 전투 상태</param>
        /// <param name="rules">게임 규칙</param>
        /// <param name="caster">스킬 시전자</param>
        /// <param name="target">스킬 대상</param>
        /// <param name="skill">사용된 스킬 데이터</param>
        /// <param name="effect">적용할 스킬 효과 데이터</param>
        /// <returns>효과가 적용된 새로운 BattleState</returns>
        BattleState Execute(BattleState battleState, DeterministicBattleRules rules, DeterministicCharacterData caster, DeterministicCharacterData target, SkillData skill, SkillEffectData effect);
    }
} 