using DungeonMaster.Battle;
using DungeonMaster.Character;
using DungeonMaster.Data;
using UnityEngine;
using System.Linq; // FirstOrDefault 사용을 위해 추가
using System.Collections.Generic;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 버프 효과 전략
    /// </summary>
    public class BuffStrategy : ISkillEffectStrategy
    {
        public BattleState Execute(BattleState battleState, DeterministicBattleRules rules, DeterministicCharacterData caster, DeterministicCharacterData target, SkillData skill, SkillEffectData effect)
        {
            if (target.CurrentHP <= 0)
            {
                return battleState; // 죽은 대상에게는 버프를 걸 수 없음, 상태 변경 없음
            }

            var buffId = effect.BuffId;
            if (buffId <= 0) return battleState;

            var blueprint = Buffs.BuffManager.Instance.GetBlueprint(buffId);
            if (blueprint == null) return battleState;

            var buffEffect = Buffs.BuffEffectRegistry.Get(buffId);
            if (buffEffect == null)
            {
                Debug.LogWarning($"ID가 {buffId}인 버프 효과(IBuffEffect)를 찾을 수 없습니다.");
                return battleState;
            }
            
            var nextState = battleState.Clone();
            var mutableTarget = nextState.GetCombatant(target.InstanceId) as DeterministicCharacterData;
            if (mutableTarget == null) return nextState;
            
            var existingBuff = mutableTarget.ActiveBuffs.FirstOrDefault(b => b.BuffId == buffId);
            
            if (existingBuff != null) // 이미 버프가 걸려있을 경우
            {
                int newStacks = existingBuff.Stacks;
                if (newStacks < blueprint.MaxStacks)
                {
                    newStacks++;
                }
                existingBuff.Stacks = newStacks;

                buffEffect.OnReapply(nextState, ref existingBuff);

                existingBuff.RemainingDurationMs = (int)(blueprint.GetScaledDuration(existingBuff.Stacks) * 1000);
                
                var newEvents = new List<BattleEvent>(nextState.Events)
                {
                    new BattleEvent(BattleEventType.BuffApply, target.InstanceId, buffId, existingBuff.Stacks)
                };
                return nextState.With(newEvents: newEvents);
            }
            else // 새로운 버프 적용
            {
                var buffData = Buffs.BuffManager.Instance.CreateBuffInstance(buffId, caster, target);
                if (buffData == null)
                {
                    return battleState;
                }
                
                buffEffect.OnApply(nextState, ref buffData);
                
                mutableTarget.ActiveBuffs.Add(buffData);

                var newEvents = new List<BattleEvent>(nextState.Events)
                {
                    new BattleEvent(BattleEventType.BuffApply, target.InstanceId, buffId, 1)
                };
                return nextState.With(newEvents: newEvents);
            }
        }
    }
} 