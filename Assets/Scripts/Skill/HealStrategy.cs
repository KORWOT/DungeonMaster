using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Data;
using System.Collections.Generic;
using DungeonMaster.Utility;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 회복 효과 전략
    /// </summary>
    public class HealStrategy : ISkillEffectStrategy
    {
        public BattleState Execute(BattleState battleState, DeterministicBattleRules rules, DeterministicCharacterData caster, DeterministicCharacterData target, SkillData skill, SkillEffectData effect)
        {
            if (target.CurrentHP <= 0 || target.CurrentHP >= target.Stats.GetValueOrDefault(StatType.MaxHP, 0))
            {
                return battleState; // 이미 죽었거나 체력이 가득 찬 대상은 치유하지 않음
            }
            
            var nextState = battleState.Clone();
            var mutableTarget = nextState.GetCharacter(target.InstanceId);
            if (mutableTarget == null) return nextState;

            long healAmount = (long)effect.Values[0];
            
            mutableTarget.CurrentHP += healAmount;
            
            long maxHp = mutableTarget.Stats.GetValueOrDefault(StatType.MaxHP, 0);
            if (mutableTarget.CurrentHP > maxHp)
            {
                mutableTarget.CurrentHP = maxHp;
            }

            var newEvents = new List<BattleEvent>(nextState.Events)
            {
                new BattleEvent(BattleEventType.Heal, mutableTarget.InstanceId, healAmount)
            };
            return nextState.With(newEvents: newEvents);
        }
    }
} 