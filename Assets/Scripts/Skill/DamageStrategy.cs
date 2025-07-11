using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Collections.Generic;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 피해 효과 전략
    /// </summary>
    public class DamageStrategy : ISkillEffectStrategy
    {
        public BattleState Execute(BattleState battleState, DeterministicBattleRules rules, DeterministicCharacterData caster, DeterministicCharacterData target, SkillData skill, SkillEffectData effect)
        {
            if (target.CurrentHP <= 0)
            {
                return battleState; // 대상이 이미 죽었으면 상태 변경 없음
            }

            var nextState = battleState.Clone();
            var mutableTarget = nextState.GetCombatant(target.InstanceId) as DeterministicCharacterData;
            if (mutableTarget == null) return nextState;

            var events = new List<BattleEvent>(nextState.Events);

            for (int i = 0; i < skill.HitCount; i++)
            {
                if (mutableTarget.CurrentHP <= 0) break;

                // 데미지 계산은 상태를 변경하지 않음
                long damage = rules.DamageCalculator.Calculate(caster, mutableTarget, skill);
                
                mutableTarget.CurrentHP -= damage;
                if (mutableTarget.CurrentHP < 0)
                {
                    mutableTarget.CurrentHP = 0;
                }
                
                events.Add(new BattleEvent(BattleEventType.Damage, mutableTarget.InstanceId, damage));
            }
            
            if (mutableTarget.CurrentHP == 0)
            {
                events.Add(new BattleEvent(BattleEventType.Death, mutableTarget.InstanceId));
            }

            return nextState.With(newEvents: events);
        }
    }
} 