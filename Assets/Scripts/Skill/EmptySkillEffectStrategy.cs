using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 아무 작업도 수행하지 않는 기본 스킬 효과 전략입니다. (Null Object Pattern)
    /// 유효하지 않거나 지원되지 않는 SkillEffectType에 대한 예외를 방지하는 데 사용됩니다.
    /// </summary>
    public class EmptySkillEffectStrategy : ISkillEffectStrategy
    {
        public BattleState Execute(BattleState battleState, DeterministicBattleRules rules, DeterministicCharacterData caster, DeterministicCharacterData target, SkillData skill, SkillEffectData effect)
        {
            // 의도적으로 아무 작업도 수행하지 않음
            #if DEBUG
            System.Diagnostics.Debug.WriteLine($"[Warning] EmptySkillEffectStrategy executed for SkillEffectType '{effect.EffectType}'. This may indicate a missing strategy implementation.");
            #endif
            return battleState;
        }
    }
} 