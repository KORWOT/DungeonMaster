using DungeonMaster.Data;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// AI 캐릭터의 행동 입력을 결정하고 수집합니다.
    /// </summary>
    public class AIActionInputProvider : IActionInputProvider
    {
        public List<ActionInput> CollectActionInputs(BattleState currentState, DeterministicBattleRules battleRules)
        {
            var actions = new List<ActionInput>();

            foreach (var characterData in currentState.Characters)
            {
                // AI는 플레이어가 아닌 캐릭터만 조작합니다.
                // 또한 살아있는 캐릭터만 행동할 수 있습니다.
                if (characterData.IsPlayerCharacter || characterData.CurrentHP <= 0) continue;

                // 스킬 사용 로직 (첫 번째 사용 가능한 스킬만 사용하도록 단순화)
                var skillToUse = characterData.Skills.FirstOrDefault(s => 
                    !characterData.SkillCooldowns.ContainsKey(s.SkillId) || characterData.SkillCooldowns[s.SkillId] <= 0);

                if (skillToUse != null)
                {
                    var targetData = FindBestTargetFor(characterData, currentState, battleRules);
                    if (targetData != null)
                    {
                        actions.Add(new ActionInput
                        {
                            Type = ActionType.Skill,
                            ActorInstanceId = characterData.InstanceId,
                            TargetInstanceIds = new List<long> { targetData.InstanceId },
                            SkillToUse = skillToUse
                        });
                        continue; // 스킬을 사용했으면 기본 공격은 하지 않음
                    }
                }

                // 기본 공격 로직
                if (characterData.AttackCooldownRemainingMs <= 0)
                {
                    var targetData = FindBestTargetFor(characterData, currentState, battleRules);
                    if (targetData != null)
                    {
                        actions.Add(new ActionInput
                        {
                            Type = ActionType.Attack,
                            ActorInstanceId = characterData.InstanceId,
                            TargetInstanceIds = new List<long> { targetData.InstanceId }
                        });
                    }
                }
            }
            
            return actions;
        }

        /// <summary>
        /// 지정된 캐릭터를 위한 최적의 공격 대상을 찾습니다.
        /// </summary>
        private DeterministicCharacterData FindBestTargetFor(
            DeterministicCharacterData attackerData, 
            BattleState currentState,
            DeterministicBattleRules battleRules)
        {
            // 적 목록 필터링: 공격자와 다른 편이고 살아있는 캐릭터
            var potentialTargets = currentState.Characters
                .Where(c => c.IsPlayerCharacter != attackerData.IsPlayerCharacter && c.CurrentHP > 0)
                .ToList();

            if (potentialTargets.Count == 0)
            {
                return null;
            }

            // 향후 더 복잡한 타겟팅 로직(도발, 어그로 순위 등) 확장 예정

            // 결정론적 Random을 사용하여 랜덤한 적을 선택
            int index = battleRules.GetRandomInt(0, potentialTargets.Count);
            return potentialTargets[index];
        }
    }
} 