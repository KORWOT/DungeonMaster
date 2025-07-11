using DungeonMaster.Data;
using DungeonMaster.Character;
using DungeonMaster.Skill;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 기본 공격을 처리하는 액션 클래스입니다.
    /// 기본 공격도 스킬 시스템을 통해 처리하여 로직을 통합합니다.
    /// </summary>
    public class AttackAction : IAction
    {
        private readonly long _actorId;
        private readonly long _targetId;

        public AttackAction(long actorId, long targetId)
        {
            _actorId = actorId;
            _targetId = targetId;
        }

        public (BattleState newState, List<BattleEvent> newEvents) Execute(BattleState currentState, DeterministicBattleRules rules)
        {
            var nextState = currentState.Clone(); 
            var generatedEvents = new List<BattleEvent>();

            var actor = nextState.GetCombatant(_actorId) as DeterministicCharacterData;
            var target = nextState.GetCombatant(_targetId) as DeterministicCharacterData;

            if (actor == null || target == null || target.CurrentHP <= 0)
            {
                return (nextState, generatedEvents); 
            }

            // 기본 공격 스킬 데이터 가져오기
            var basicAttackSkill = SkillManager.Instance.GetBasicAttackSkill();
            if (basicAttackSkill == null)
            {
                // 기본 공격 스킬이 없으면 최소 데미지 1로 처리
                target.CurrentHP -= 1;
                generatedEvents.Add(new BattleEvent(BattleEventType.Damage, _targetId, 1));
                return (nextState, generatedEvents);
            }

            // 데미지 계산
            long damage = rules.DamageCalculator.Calculate(actor, target, basicAttackSkill);

            // 데미지 적용
            target.CurrentHP -= damage;
            if (target.CurrentHP < 0)
            {
                target.CurrentHP = 0;
            }
            
            // 이벤트 생성
            generatedEvents.Add(new BattleEvent(BattleEventType.Damage, _targetId, damage));
            if(target.CurrentHP == 0)
            {
                generatedEvents.Add(new BattleEvent(BattleEventType.Death, _targetId));
            }
            
            // 공격 쿨다운 설정
            long attackSpeed = actor.Stats.GetValueOrDefault(StatType.AttackSpeed, 100); // 기본 1.0초당 1회
            actor.AttackCooldownRemainingMs = 1000 * 100 / attackSpeed; 

            return (nextState, generatedEvents);
        }
    }
} 