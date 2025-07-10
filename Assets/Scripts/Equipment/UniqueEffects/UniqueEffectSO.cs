using UnityEngine;
using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Data; // DeterministicCharacterData를 위해 추가
using DungeonMaster.Battle;
using DungeonMaster.Utility;
using DungeonMaster.Localization;
using System.Linq;

namespace DungeonMaster.Equipment
{
    // --- 데이터 구조 및 열거형 ---

    public enum ValueSource
    {
        FixedValue,         // 고정값 사용
        EquipmentPrimary,   // 장비의 Primary Value 사용
        EquipmentSecondary, // 장비의 Secondary Value 사용
    }

    [System.Serializable]
    public struct ScalableFloat
    {
        public ValueSource Source;
        public float FixedValue;
        // TODO: 스케일링 로직 추가

        public float GetValue(BaseMonsterEquipment equipment)
        {
            if (equipment == null) return FixedValue;
            switch (Source)
            {
                case ValueSource.EquipmentPrimary: return equipment.UniqueEffectPrimaryValue;
                case ValueSource.EquipmentSecondary: return equipment.UniqueEffectSecondaryValue;
                case ValueSource.FixedValue:
                default:
                    return FixedValue;
            }
        }
    }

    /// <summary>
    /// 효과가 발생하는 시점을 정의하는 열거형입니다.
    /// </summary>
    public enum EffectTriggerType
    {
        OnApply,            // 장비 장착 시
        OnRemove,           // 장비 해제 시
        OnDamageDealt,      // 피해를 입혔을 때
        OnCriticalHit,      // 치명타를 입혔을 때
        OnTakeDamage,       // 피해를 받았을 때
        OnKill,             // 적을 처치했을 때
    }

    /// <summary>
    /// 효과 발동 조건을 검사할 대상을 지정합니다.
    /// </summary>
    public enum ConditionTarget
    {
        Source,     // 효과를 유발한 주체 (예: 공격자)
        Target,     // 효과의 대상 (예: 피격자)
    }

    /// <summary>
    /// 효과 발동 조건에 사용될 비교 연산자입니다.
    /// </summary>
    public enum ConditionOperator
    {
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equal,
        NotEqual
    }

    // --- 행동 부품(Component)들의 기반 클래스 ---

    /// <summary>
    /// 효과 발동을 위한 단일 조건을 나타내는 추상 클래스입니다.
    /// 이 클래스를 상속받아 구체적인 조건들을 구현합니다.
    /// </summary>
    [System.Serializable]
    public abstract class EffectCondition
    {
        /// <summary>
        /// 이 조건이 충족되었는지 확인합니다.
        /// </summary>
        public abstract bool IsMet(DeterministicCharacterData source, DeterministicCharacterData target, BaseMonsterEquipment equipment, DamageContext context);
    }

    /// <summary>
    /// 효과가 발동했을 때 수행될 단일 행동을 나타내는 추상 클래스입니다.
    /// </summary>
    [System.Serializable]
    public abstract class EffectAction
    {
        /// <summary>
        /// 행동을 실행합니다.
        /// </summary>
        public abstract void Execute(DeterministicCharacterData source, DeterministicCharacterData target, BaseMonsterEquipment equipment);
    }
    
    // --- 조건(Condition) 구현체들 ---
    
    [System.Serializable]
    public class HealthCondition : EffectCondition
    {
        public ConditionTarget CheckTarget = ConditionTarget.Source;
        public ConditionOperator Operator;
        [Range(0, 100)] public float HealthPercent;

        public override bool IsMet(DeterministicCharacterData source, DeterministicCharacterData target, BaseMonsterEquipment equipment, DamageContext context)
        {
            var characterToCheck = CheckTarget == ConditionTarget.Source ? source : target;
            if (characterToCheck == null) return false;

            long maxHp = characterToCheck.Stats.GetValueOrDefault(StatType.MaxHP, 1);
            if (maxHp == 0) return false;
            
            float currentHpPercent = (float)characterToCheck.CurrentHP / maxHp * 100;

            switch (Operator)
            {
                case ConditionOperator.LessThan: return currentHpPercent < HealthPercent;
                case ConditionOperator.GreaterThan: return currentHpPercent > HealthPercent;
                case ConditionOperator.LessThanOrEqual: return currentHpPercent <= HealthPercent;
                case ConditionOperator.GreaterThanOrEqual: return currentHpPercent >= HealthPercent;
                case ConditionOperator.Equal: return Mathf.Approximately(currentHpPercent, HealthPercent);
                case ConditionOperator.NotEqual: return !Mathf.Approximately(currentHpPercent, HealthPercent);
                default: return false;
            }
        }
    }

    // --- 행동(Action) 구현체들 ---
    
    [System.Serializable]
    public class ModifyStatAction : EffectAction
    {
        public StatType StatToModify;
        public ScalableFloat Value;

        public override void Execute(DeterministicCharacterData source, DeterministicCharacterData target, BaseMonsterEquipment equipment)
        {
            if (target == null) return;
            
            long finalValue = (long)Value.GetValue(equipment); 
            target.Stats[StatToModify] += finalValue;
        }
    }
    
    // --- 규칙(Rule) 정의 ---
    
    [System.Serializable]
    public class TriggeredRule
    {
        public EffectTriggerType Trigger;

        [SerializeReference]
        public List<EffectCondition> Conditions = new List<EffectCondition>();
        
        [SerializeReference]
        public List<EffectAction> Actions = new List<EffectAction>();
    }

    [System.Serializable]
    public class DamageModification
    {
        public enum ModificationType { Additive, Multiplicative }
        public ModificationType Type = ModificationType.Multiplicative;
        public ScalableFloat Value;
    }
    
    [System.Serializable]
    public class DamageModifierRule
    {
        public string RuleName; // 인스펙터에서 알아보기 위함
        
        [SerializeReference]
        public List<EffectCondition> Conditions = new List<EffectCondition>();
        
        public List<DamageModification> Modifications = new List<DamageModification>();
    }

    // --- 메인 ScriptableObject 클래스 ---

    /// <summary>
    /// 데이터 기반으로 조합 가능한 고유 효과를 정의하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUniqueEffect", menuName = "DungeonMaster/Equipment/Unique Effect")]
    public class UniqueEffectSO : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("효과 이름 로컬라이제이션 키")]
        public string effectNameKey;
        
        [Tooltip("효과 설명 로컬라이제이션 키. 동적 설명이 필요하면 비워둘 수 있습니다.")]
        public string baseDescriptionKey;

        [Header("효과 규칙")]
        public List<TriggeredRule> Rules = new List<TriggeredRule>();
        
        [Header("데미지 수정 규칙")]
        public List<DamageModifierRule> DamageRules = new List<DamageModifierRule>();

        public int Priority => 100; // IDamageModifier 구현

        /// <summary>
        /// 장비 장착 시 이 효과를 적용합니다.
        /// </summary>
        public void OnApply(DeterministicCharacterData target, BaseMonsterEquipment equipment)
        {
            HandleEvent(EffectTriggerType.OnApply, target, target, equipment, null);
        }

        /// <summary>
        /// 장비 해제 시 이 효과를 제거합니다.
        /// </summary>
        public void OnRemove(DeterministicCharacterData target, BaseMonsterEquipment equipment)
        {
            HandleEvent(EffectTriggerType.OnRemove, target, target, equipment, null);
        }

        /// <summary>
        /// 공격 시 피해량을 수정합니다.
        /// </summary>
        public long ModifyDamage(long initialDamage, DeterministicCharacterData attacker, DeterministicCharacterData defender, BaseMonsterEquipment equipment, DamageContext context)
        {
            long additiveBonus = 0;
            double multiplicativeBonus = 1.0;

            foreach (var rule in DamageRules)
            {
                bool allConditionsMet = true;
                foreach (var condition in rule.Conditions)
                {
                    if (!condition.IsMet(attacker, defender, equipment, context))
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                if (allConditionsMet)
                {
                    foreach (var mod in rule.Modifications)
                    {
                        float value = mod.Value.GetValue(equipment);
                        if (mod.Type == DamageModification.ModificationType.Additive)
                        {
                            additiveBonus += (long)value;
                        }
                        else // Multiplicative
                        {
                            multiplicativeBonus *= (1.0 + value / 100.0);
                        }
                    }
                }
            }

            // 최종 계산: (기본 데미지 * 곱연산) + 합연산
            long finalDamage = (long)(initialDamage * multiplicativeBonus) + additiveBonus;
            return finalDamage;
        }
        
        /// <summary>
        /// 특정 이벤트가 발생했을 때 이 효과를 처리합니다.
        /// </summary>
        public void HandleEvent(EffectTriggerType trigger, DeterministicCharacterData source, DeterministicCharacterData target, BaseMonsterEquipment equipment, DamageContext context)
        {
            foreach (var rule in Rules)
            {
                if (rule.Trigger == trigger)
                {
                    bool allConditionsMet = true;
                    foreach (var condition in rule.Conditions)
                    {
                        if (!condition.IsMet(source, target, equipment, context))
                        {
                            allConditionsMet = false;
                            break;
                        }
                    }

                    if (allConditionsMet)
                    {
                        foreach (var action in rule.Actions)
                        {
                            action.Execute(source, target, equipment);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 이 효과에 대한 동적 설명을 생성합니다.
        /// </summary>
        public string GetDescription(BaseMonsterEquipment equipment)
        {
            // TODO: 규칙 기반 동적 설명 생성 로직 구현
            if (!string.IsNullOrEmpty(baseDescriptionKey))
            {
                 return LocalizationManager.Instance.GetText(baseDescriptionKey);
            }
            return "동적 설명 생성 필요";
        }
    }
} 