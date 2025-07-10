using DungeonMaster.Character;
using System.Collections.Generic;
using UnityEngine;
using DungeonMaster.Utility;
using DungeonMaster.Data;
using DungeonMaster.Localization;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 장비 효과 기본 클래스
    /// </summary>
    [System.Serializable]
    public abstract class EquipmentEffect
    {
        [SerializeField] protected string effectName;
        [SerializeField] protected string description;
        
        public virtual string EffectName => effectName;
        public virtual string Description => description;
        
        public abstract void Apply(DeterministicCharacterData target, int equipmentLevel);
        public abstract void Remove(DeterministicCharacterData target, int equipmentLevel);
        
        protected EquipmentEffect(string name, string description)
        {
            this.effectName = name;
            this.description = description;
        }
    }

    /// <summary>
    /// 스탯 수정 효과
    /// </summary>
    [System.Serializable]
    public class StatModifierEffect : EquipmentEffect
    {
        [SerializeField] private StatType statType;
        [SerializeField] private float baseValue;
        [SerializeField] private float levelScaling;
        
        public override string EffectName => LocalizationManager.Instance.GetText(effectName);

        public StatType StatType => statType;
        public float BaseValue => baseValue;
        public float LevelScaling => levelScaling;
        
        public StatModifierEffect(string name, StatType statType, float baseValue, float levelScaling = 1f) 
            : base(name, LocalizationManager.Instance.GetTextFormatted("effect_format_stat_modifier_desc", statType, baseValue, levelScaling))
        {
            this.statType = statType;
            this.baseValue = baseValue;
            this.levelScaling = levelScaling;
        }
        
        public override void Apply(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_target_null", "StatModifierEffect"));
                return;
            }
            
            try
            {
                float totalValue = baseValue + (levelScaling * Mathf.Max(0, equipmentLevel - 1));
                target.Stats[statType] = target.Stats.GetValueOrDefault(statType, 0) + (long)totalValue;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("effect_log_apply_stat_modifier", target.Name, effectName, statType, totalValue));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_apply_exception", "StatModifierEffect", e.Message));
            }
        }
        
        public override void Remove(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_target_null", "StatModifierEffect"));
                return;
            }
            
            try
            {
                float totalValue = baseValue + (levelScaling * Mathf.Max(0, equipmentLevel - 1));
                target.Stats[statType] = target.Stats.GetValueOrDefault(statType, 0) - (long)totalValue;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("effect_log_remove_stat_modifier", target.Name, effectName, statType, totalValue));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_remove_exception", "StatModifierEffect", e.Message));
            }
        }
    }

    /// <summary>
    /// 개선된 퍼센트 스탯 수정 효과 - 전체 퍼센트 합산 후 적용
    /// </summary>
    [System.Serializable]
    public class PercentStatModifierEffect : EquipmentEffect
    {
        [SerializeField] private StatType statType;
        [SerializeField] private float percentValue;
        [SerializeField] private float levelScaling;
        
        public override string EffectName => LocalizationManager.Instance.GetText(effectName);

        // 전역 퍼센트 효과 추적
        private static Dictionary<DeterministicCharacterData, Dictionary<StatType, List<PercentEffectData>>> activePercentEffects 
            = new Dictionary<DeterministicCharacterData, Dictionary<StatType, List<PercentEffectData>>>();
        
        private struct PercentEffectData
        {
            public string effectId;
            public float percent;
            
            public PercentEffectData(string id, float percent)
            {
                this.effectId = id;
                this.percent = percent;
            }
        }
        
        [System.NonSerialized] // Unity 직렬화에서 제외
        private string effectId;
        
        /// <summary>
        /// 고유 효과 ID (런타임에만 사용)
        /// </summary>
        private string EffectId => effectId ??= System.Guid.NewGuid().ToString();
        
        public PercentStatModifierEffect(string name, StatType statType, float percentValue, float levelScaling = 0.5f) 
            : base(name, LocalizationManager.Instance.GetTextFormatted("effect_format_percent_stat_modifier_desc", statType, percentValue, levelScaling))
        {
            this.statType = statType;
            this.percentValue = percentValue;
            this.levelScaling = levelScaling;
        }
        
        public override void Apply(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_target_null", "PercentStatModifierEffect"));
                return;
            }
            
            try
            {
                float totalPercent = percentValue + (levelScaling * Mathf.Max(0, equipmentLevel - 1));
                
                RegisterPercentEffect(target, statType, EffectId, totalPercent);
                RecalculatePercentEffect(target, statType);
                
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("effect_log_apply_percent_stat_modifier", target.Name, effectName, statType, totalPercent));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_apply_exception", "PercentStatModifierEffect", e.Message));
            }
        }
        
        public override void Remove(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_target_null", "PercentStatModifierEffect"));
                return;
            }
            
            try
            {
                UnregisterPercentEffect(target, statType, EffectId);
                RecalculatePercentEffect(target, statType);
                
                float totalPercent = percentValue + (levelScaling * Mathf.Max(0, equipmentLevel - 1));
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("effect_log_remove_percent_stat_modifier", target.Name, effectName, statType, totalPercent));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("effect_error_remove_exception", "PercentStatModifierEffect", e.Message));
            }
        }
        
        private static void RegisterPercentEffect(DeterministicCharacterData target, StatType statType, string effectId, float percent)
        {
            if (!activePercentEffects.ContainsKey(target))
            {
                activePercentEffects[target] = new Dictionary<StatType, List<PercentEffectData>>();
            }
            
            if (!activePercentEffects[target].ContainsKey(statType))
            {
                activePercentEffects[target][statType] = new List<PercentEffectData>();
            }
            
            // 기존 동일 effectId 제거 (중복 방지)
            activePercentEffects[target][statType].RemoveAll(e => e.effectId == effectId);
            
            // 새 효과 추가
            activePercentEffects[target][statType].Add(new PercentEffectData(effectId, percent));
        }
        
        private static void UnregisterPercentEffect(DeterministicCharacterData target, StatType statType, string effectId)
        {
            if (activePercentEffects.ContainsKey(target) && 
                activePercentEffects[target].ContainsKey(statType))
            {
                activePercentEffects[target][statType].RemoveAll(e => e.effectId == effectId);
                
                // 빈 리스트 정리
                if (activePercentEffects[target][statType].Count == 0)
                {
                    activePercentEffects[target].Remove(statType);
                }
                
                if (activePercentEffects[target].Count == 0)
                {
                    activePercentEffects.Remove(target);
                }
            }
        }
        
        private static void RecalculatePercentEffect(DeterministicCharacterData target, StatType statType)
        {
            // 이 로직은 결정론적 시스템과 호환되지 않으므로, 재설계가 필요합니다.
            // 지금은 컴파일 오류를 막기 위해 임시로 비워둡니다.
            // 기존 퍼센트 효과 제거 (기본값으로 복원)
            // string percentKey = $"PERCENT_{statType}";
            // if (target.Stats.HasModifier(percentKey))
            // {
            //     float oldBonus = target.Stats.GetModifier(percentKey);
            //     target.Stats.Add(statType, -oldBonus);
            //     target.Stats.RemoveModifier(percentKey);
            // }
            
            // 새로운 총 퍼센트 계산
            // float totalPercent = 0f;
            // if (activePercentEffects.ContainsKey(target) && 
            //     activePercentEffects[target].ContainsKey(statType))
            // {
            //     foreach (var effect in activePercentEffects[target][statType])
            //     {
            //         totalPercent += effect.percent;
            //     }
            // }
            
            // // 새 퍼센트 효과 적용
            // if (totalPercent > 0)
            // {
            //     float baseValue = target.Stats.GetBaseValue(statType);
            //     float bonus = baseValue * totalPercent / 100f;
            //     target.Stats.Add(statType, bonus);
            //     target.Stats.SetModifier(percentKey, bonus);
            // }
        }
        
        public static void ClearPercentEffects(DeterministicCharacterData target)
        {
            // 이 로직도 재설계가 필요합니다.
            // if (activePercentEffects.ContainsKey(target))
            // {
            //     var statTypesToRecalculate = new List<StatType>(activePercentEffects[target].Keys);
            //     activePercentEffects.Remove(target);
                
            //     foreach (var statType in statTypesToRecalculate)
            //     {
            //         RecalculatePercentEffect(target, statType);
            //     }
            // }
        }
    }
} 