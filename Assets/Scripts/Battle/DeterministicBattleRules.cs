using System;
using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Data;
using DungeonMaster.Buffs;
using DungeonMaster.Battle.Damage;

namespace DungeonMaster.Battle
{
    /// <summary>
    /// 결정론적 전투의 모든 규칙과 계산을 담당하는 '두뇌' 클래스입니다.
    /// 이 클래스는 UnityEngine에 의존하지 않는 순수 C# 로직으로만 구성됩니다.
    /// </summary>
    public class DeterministicBattleRules
    {
        private readonly DeterministicBattleSettingsData _settings;
        private readonly Random _random;
        private readonly IDamageCalculator _damageCalculator;
        
        public IDamageCalculator DamageCalculator => _damageCalculator;

        /// <summary>
        /// 생성자에서 전투 규칙 설정과 결정론적 난수 시드(seed)를 주입받습니다.
        /// </summary>
        public DeterministicBattleRules(DeterministicBattleSettingsData settings, 
                                    int seed, 
                                    ElementalAffinityTable elementalAffinityTable,
                                    DamageSettings damageSettings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _random = new Random(seed);

            // 새로운 데미지 계산 파이프라인 구성 (이전 공식 기반)
            var pipeline = new List<IDamageCalculationStep>
            {
                // --- 주는 피해량 계산 ---
                new BaseDamageStep(),
                new CriticalDamageStep(_random),
                new ElementalBonusDamageStep(),
                new BonusDamageStep(),
                
                // --- 받는 피해량 계산 ---
                new PenetrationStep(), // 방어력 계산을 먼저 수행
                new DefenseStep(),     // 계산된 방어력 적용
                new ReductionStep(),   // 각종 피해 감소 적용
                new ElementalAffinityStep(elementalAffinityTable), // 속성 상성 적용
                
                // --- 최종 보정 ---
                new ModifierStep() // 장비 고유 효과 등 최종 수정자 적용
            };
            
            _damageCalculator = new DefaultDamageCalculator(pipeline);
        }

        /// <summary>
        /// 한 틱(프레임) 동안의 전투를 처리하고, 다음 상태를 반환합니다.
        /// 이 메서드는 '순수 함수'처럼 동작하여, 외부 상태를 변경하지 않고 오직 결과물(새로운 상태)만 반환합니다.
        /// </summary>
        public (BattleState, List<BattleEvent>) ProcessTick(BattleState currentState, List<IAction> actions, long deltaTimeMs)
        {
            // 1. 상태를 직접 수정하지 않고, 복사본을 만들어 사용합니다. (불변성 유지)
            var nextState = currentState.Clone();
            nextState.CurrentTimeMs += deltaTimeMs;

            // 2. 버프/디버프 처리 (지속시간 감소, 도트 데미지/힐 적용)
            nextState = ApplyAllBuffEffects(nextState, deltaTimeMs);

            // 3. 쿨타임 처리 (스킬 및 기본 공격)
            nextState = UpdateAllCooldowns(nextState, deltaTimeMs);
            
            var allEvents = new List<BattleEvent>(nextState.Events);

            // 4. 입력된 액션들 실행
            foreach (var action in actions)
            {
                var (stateAfterAction, eventsFromAction) = action.Execute(nextState, this);
                nextState = stateAfterAction;
                if (eventsFromAction != null)
                {
                    allEvents.AddRange(eventsFromAction);
                }
            }
            
            nextState.Events.Clear();
            nextState.Events.AddRange(allEvents);
    
            // 5. 모든 계산이 끝난 새로운 상태와 발생한 이벤트를 반환합니다.
            return (nextState, nextState.Events);
        }

        /// <summary>
        /// 상태에 있는 모든 전투원의 버프 효과를 적용하고 지속시간을 갱신합니다.
        /// </summary>
        private BattleState ApplyAllBuffEffects(BattleState state, long deltaTimeMs)
        {
            var nextState = state;
            
            var updatedCharacters = ApplyBuffsToCombatantList(nextState, nextState.Characters, deltaTimeMs);
            var updatedDemonLords = ApplyBuffsToCombatantList(nextState, nextState.DemonLords, deltaTimeMs);

            return nextState.With(newCharacters: updatedCharacters, newDemonLords: updatedDemonLords);
        }
        
        private List<T> ApplyBuffsToCombatantList<T>(BattleState state, IEnumerable<T> combatants, long deltaTimeMs) where T : class, ICombatantData
        {
            var updatedList = new List<T>();

            foreach (var combatant in combatants)
            {
                if (combatant.ActiveBuffs.Count == 0)
                {
                    updatedList.Add(combatant);
                    continue;
                }

                var newBuffList = new List<Data.BuffData>();
                
                foreach (var buff in combatant.ActiveBuffs)
                {
                    var effect = BuffEffectRegistry.Get(buff.BuffId);
                    if (effect == null) continue;

                    var mutableBuff = buff; 
                    
                    effect.OnTick(state, ref mutableBuff);
                    
                    mutableBuff.RemainingDurationMs -= (int)deltaTimeMs;

                    if (mutableBuff.RemainingDurationMs > 0)
                    {
                        newBuffList.Add(mutableBuff);
                    }
                    else
                    {
                        effect.OnRemove(state, mutableBuff);
                        state.Events.Add(new BattleEvent(BattleEventType.BuffRemove, combatant.InstanceId, mutableBuff.BuffId));
                    }
                }

                combatant.ActiveBuffs = newBuffList;
                updatedList.Add(combatant);
            }
            
            return updatedList;
        }


        /// <summary>
        /// 상태에 있는 모든 전투원의 스킬 및 공격 쿨타임을 갱신합니다.
        /// </summary>
        private BattleState UpdateAllCooldowns(BattleState state, long deltaTimeMs)
        {
            var updatedCharacters = UpdateCooldownsForCombatantList(state.Characters, deltaTimeMs);
            var updatedDemonLords = UpdateCooldownsForCombatantList(state.DemonLords, deltaTimeMs);
            
            return state.With(newCharacters: updatedCharacters, newDemonLords: updatedDemonLords);
        }
        
        private List<T> UpdateCooldownsForCombatantList<T>(IEnumerable<T> combatants, long deltaTimeMs) where T : class, ICombatantData
        {
            var updatedList = new List<T>();

            foreach (var combatant in combatants)
            {
                // 공격 쿨타임 감소
                if (combatant.AttackCooldownRemainingMs > 0)
                {
                    combatant.AttackCooldownRemainingMs = System.Math.Max(0, combatant.AttackCooldownRemainingMs - deltaTimeMs);
                }

                // 스킬 쿨타임 감소
                if (combatant.SkillCooldowns != null && combatant.SkillCooldowns.Count > 0)
                {
                    var newCooldowns = new Dictionary<long, long>();
                    foreach (var cooldown in combatant.SkillCooldowns)
                    {
                        var remaining = cooldown.Value - deltaTimeMs;
                        if (remaining > 0)
                        {
                            newCooldowns[cooldown.Key] = remaining;
                        }
                    }
                    combatant.SkillCooldowns = newCooldowns;
                }
                updatedList.Add(combatant);
            }
            return updatedList;
        }
        
        /// <summary>
        /// 결정론적인 정수 난수를 반환합니다.
        /// </summary>
        public int GetRandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
} 