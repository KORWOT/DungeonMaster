using DungeonMaster.Data;
using DungeonMaster.Skill;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// SkillAction class represents an action that uses a skill.
    /// </summary>
    public class SkillAction : IAction
    {
        private readonly long _casterId;
        private readonly long _targetId;
        private readonly SkillData _skill;

        public SkillAction(long casterId, long targetId, SkillData skill)
        {
            _casterId = casterId;
            _targetId = targetId;
            _skill = skill;
        }

        public (BattleState newState, List<BattleEvent> newEvents) Execute(BattleState currentState, DeterministicBattleRules battleRules)
        {
            var nextState = currentState.Clone();
            var caster = nextState.GetCombatant(_casterId) as DeterministicCharacterData;
            var target = nextState.GetCombatant(_targetId) as DeterministicCharacterData;

            if (caster == null || target == null || _skill == null)
            {
                return (currentState, new List<BattleEvent>());
            }

            // 스킬 효과 실행
            foreach (var effect in _skill.GetScaledEffects((int)caster.GetSkillLevel(_skill.SkillId)))
            {
                var strategy = SkillEffectStrategyFactory.Instance.GetStrategy(effect.EffectType);
                nextState = strategy.Execute(nextState, battleRules, caster, target, _skill, effect);
                
                // 루프를 돌 때마다 변경된 상태에서 캐릭터 정보를 다시 가져와야 합니다.
                caster = nextState.GetCombatant(_casterId) as DeterministicCharacterData;
                target = nextState.GetCombatant(_targetId) as DeterministicCharacterData;
            }

            // 쿨다운 설정
            var mutableCaster = nextState.GetCombatant(_casterId) as DeterministicCharacterData;
            if (mutableCaster != null)
            {
                mutableCaster.AttackCooldownRemainingMs = mutableCaster.Stats.GetValueOrDefault(Character.StatType.AttackSpeed, 1000);

                var scaledCooldownMs = (long)(_skill.GetScaledCooldown((int)caster.GetSkillLevel(_skill.SkillId)) * 1000);
                if (scaledCooldownMs > 0)
                {
                    mutableCaster.SkillCooldowns[_skill.SkillId] = scaledCooldownMs;
                }
            }
            
            var events = nextState.Events.Except(currentState.Events).ToList();
            return (nextState, events);
        }
    }
} 