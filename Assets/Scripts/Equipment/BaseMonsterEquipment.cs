using System.Collections.Generic;
using System.Text;
using DungeonMaster.Character;
using DungeonMaster.Data;
using DungeonMaster.Battle; // IDamageModifier, DamageContext를 위해 추가
using DungeonMaster.Localization;
using DungeonMaster.Utility;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 장비 기본 ScriptableObject
    /// </summary>
    public abstract class BaseMonsterEquipment : ScriptableObject, IMonsterEquipment, IDamageModifier
    {
        private const string UnknownEquipmentKey = "equip_const_unknown";
        private const string NoDescriptionKey = "equip_const_no_desc";

        [Header("기본 정보")]
        [SerializeField] private long equipmentId;
        [SerializeField] private string equipmentName;
        [SerializeField] private string description;
        [SerializeField] private MonsterEquipmentType equipmentType;

        [Header("등급과 레벨")]
        [SerializeField] private EquipmentGrade grade = EquipmentGrade.Normal;
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 20;

        [Header("기본 효과 (장비 타입별 고정)")]
        [SerializeField] private List<EquipmentEffect> baseEffects = new List<EquipmentEffect>();

        [Header("추가 효과 (등급에 따라 증가)")]
        [SerializeField] private List<EquipmentEffect> additionalEffects = new List<EquipmentEffect>();

        [Header("고유 효과 (에픽 이상)")]
        [SerializeField] private UniqueEffectSO uniqueEffectSO;
        public float UniqueEffectPrimaryValue;
        public float UniqueEffectSecondaryValue;

        #region IDamageModifier Implementation

        public int Priority => uniqueEffectSO != null ? 100 : int.MaxValue; // 유니크 효과가 없으면 우선순위를 최하로

        public long ModifyDamage(long damage, DeterministicCharacterData attacker, DeterministicCharacterData defender, DamageContext context)
        {
            if (uniqueEffectSO != null)
            {
                return uniqueEffectSO.ModifyDamage(damage, attacker, defender, this, context);
            }
            return damage;
        }

        #endregion

        public Dictionary<StatType, long> GetAllStatBonuses(int atLevel)
        {
            var totalBonuses = new Dictionary<StatType, long>();
            var calculatedLevel = Mathf.Max(0, atLevel - 1);

            ProcessEffectList(BaseEffects, calculatedLevel, totalBonuses);
            ProcessEffectList(AdditionalEffects, calculatedLevel, totalBonuses);

            return totalBonuses;
        }

        private void ProcessEffectList(IEnumerable<EquipmentEffect> effects, int calculatedLevel, IDictionary<StatType, long> totalBonuses)
        {
            foreach (var effect in effects)
            {
                if (effect is not StatModifierEffect statEffect) continue;
                
                long value = (long)(statEffect.BaseValue + (statEffect.LevelScaling * calculatedLevel));
                totalBonuses.TryGetValue(statEffect.StatType, out var currentValue);
                totalBonuses[statEffect.StatType] = currentValue + value;
            }
        }

        public long EquipmentId => equipmentId;
        public string Name => LocalizationManager.Instance.GetText(string.IsNullOrEmpty(equipmentName) ? UnknownEquipmentKey : equipmentName);
        public string Description => LocalizationManager.Instance.GetText(string.IsNullOrEmpty(description) ? NoDescriptionKey : description);
        public MonsterEquipmentType EquipmentType => equipmentType;
        public EquipmentGrade Grade => grade;
        public int Level => level;
        public int MaxLevel => maxLevel;
        public List<EquipmentEffect> BaseEffects => baseEffects ??= new List<EquipmentEffect>();
        public List<EquipmentEffect> AdditionalEffects => additionalEffects ??= new List<EquipmentEffect>();
        public UniqueEffectSO UniqueEffectSO => uniqueEffectSO;

        public int RecommendedAdditionalEffects => grade switch
        {
            EquipmentGrade.Normal => 0,
            EquipmentGrade.Magic => 1,
            EquipmentGrade.Rare => 3,
            EquipmentGrade.Epic => 6,
            EquipmentGrade.Legendary => 10,
            _ => 0
        };

        public bool CanHaveUniqueEffect => grade >= EquipmentGrade.Epic;

        public virtual void ApplyTo(DeterministicCharacterData characterData)
        {
            if (characterData?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_target_null", Name));
                return;
            }

            if (!ValidateEquipment())
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_validation_failed", Name));
                return;
            }

            try
            {
                var lm = LocalizationManager.Instance;
                ApplyEffectList(BaseEffects, characterData, lm.GetText("equip_ui_label_base_effects"));
                ApplyEffectList(AdditionalEffects, characterData, lm.GetText("equip_ui_label_additional_effects"));

                if (CanHaveUniqueEffect && uniqueEffectSO != null)
                {
                    uniqueEffectSO.OnApply(characterData, this);
                }

                GameLogger.LogInfo(lm.GetTextFormatted("equip_log_info_apply_success", characterData.Name, Name, level, grade));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_exception", Name, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }

        public virtual void RemoveFrom(DeterministicCharacterData characterData)
        {
            if (characterData?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_remove_target_null", Name));
                return;
            }

            try
            {
                var lm = LocalizationManager.Instance;
                RemoveEffectList(BaseEffects, characterData, lm.GetText("equip_ui_label_base_effects"));
                RemoveEffectList(AdditionalEffects, characterData, lm.GetText("equip_ui_label_additional_effects"));

                if (CanHaveUniqueEffect && uniqueEffectSO != null)
                {
                    uniqueEffectSO.OnRemove(characterData, this);
                }

                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_remove_success", characterData.Name, Name));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_remove_exception", Name, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }

        public virtual void ApplyTo(ICharacter monster)
        {
            if (monster?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_target_null", Name));
                return;
            }

            if (!ValidateEquipment())
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_validation_failed", Name));
                return;
            }
            
            try
            {
                var lm = LocalizationManager.Instance;
                ApplyEffectListToCharacter(BaseEffects, monster, lm.GetText("equip_ui_label_base_effects"));
                ApplyEffectListToCharacter(AdditionalEffects, monster, lm.GetText("equip_ui_label_additional_effects"));

                if (CanHaveUniqueEffect && uniqueEffectSO != null)
                {
                    GameLogger.LogWarning(lm.GetTextFormatted("equip_log_warn_unique_effect_on_icharacter", Name));
                }

                GameLogger.LogInfo(lm.GetTextFormatted("equip_log_info_apply_success", monster.Name, Name, level, grade));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_apply_exception", Name, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }

        public virtual void RemoveFrom(ICharacter monster)
        {
            if (monster?.Stats == null)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_remove_target_null", Name));
                return;
            }

            try
            {
                var lm = LocalizationManager.Instance;
                RemoveEffectListFromCharacter(BaseEffects, monster, lm.GetText("equip_ui_label_base_effects"));
                RemoveEffectListFromCharacter(AdditionalEffects, monster, lm.GetText("equip_ui_label_additional_effects"));

                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_remove_success", monster.Name, Name));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted("equip_log_error_remove_exception", Name, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }

        private void ApplyEffectList(IEnumerable<EquipmentEffect> effects, DeterministicCharacterData characterData, string effectType)
        {
            foreach (var effect in effects)
            {
                if (effect == null) continue;
                effect.Apply(characterData, level);
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_apply_effect", effectType, effect.EffectName, effect.Description));
            }
        }

        private void RemoveEffectList(IEnumerable<EquipmentEffect> effects, DeterministicCharacterData characterData, string effectType)
        {
            foreach (var effect in effects)
            {
                if (effect == null) continue;
                effect.Remove(characterData, level);
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_remove_effect", effectType, effect.EffectName));
            }
        }

        private void ApplyEffectListToCharacter(IEnumerable<EquipmentEffect> effects, ICharacter monster, string effectType)
        {
            foreach (var effect in effects)
            {
                if (effect is not StatModifierEffect statEffect) continue;
                
                long value = (long)(statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, level - 1)));
                monster.Stats[statEffect.StatType] += value;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_apply_effect", effectType, effect.EffectName, effect.Description));
            }
        }

        private void RemoveEffectListFromCharacter(IEnumerable<EquipmentEffect> effects, ICharacter monster, string effectType)
        {
            foreach (var effect in effects)
            {
                if (effect is not StatModifierEffect statEffect) continue;
                
                long value = (long)(statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, level - 1)));
                monster.Stats[statEffect.StatType] -= value;
                GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_remove_effect", effectType, effect.EffectName));
            }
        }

        private bool ValidateEquipment()
        {
            var lm = LocalizationManager.Instance;
            if (equipmentId == 0)
            {
                GameLogger.LogError(lm.GetText("equip_validate_error_invalid_id"));
                return false;
            }

            if (string.IsNullOrEmpty(equipmentName))
            {
                GameLogger.LogError(lm.GetText("equip_validate_error_empty_name"));
                return false;
            }

            if (uniqueEffectSO != null && !CanHaveUniqueEffect)
            {
                GameLogger.LogWarning(lm.GetText("equip_validate_warn_unique_effect_grade_mismatch"));
            }

            if (AdditionalEffects.Count > RecommendedAdditionalEffects)
            {
                GameLogger.LogWarning(lm.GetText("equip_validate_warn_additional_effects_exceeded"));
            }

            return true;
        }

        public bool CanUpgrade() => level < maxLevel;

        public void UpgradeLevel()
        {
            if (!CanUpgrade())
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetTextFormatted("equip_upgrade_warn_max_level", Name, level, maxLevel));
                return;
            }
            level++;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_upgrade_info_level_up", Name, level - 1, level));
        }

        public void UpgradeGrade()
        {
            if (grade >= EquipmentGrade.Legendary)
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("equip_upgrade_warn_max_grade"));
                return;
            }
            
            var oldGrade = grade;
            grade++;
            // TODO: 등급업 로직에 대한 로컬라이제이션 키 추가 필요
            GameLogger.LogInfo($"[TODO: Localize] {Name} 등급 업그레이드 완료: {oldGrade} → {grade}");
        }

        public bool SetUniqueEffect(UniqueEffectSO effect, float primaryValue, float secondaryValue)
        {
            if (!CanHaveUniqueEffect)
            {
                GameLogger.LogWarning(LocalizationManager.Instance.GetText("equip_unique_effect_warn_grade_too_low"));
                return false;
            }
            uniqueEffectSO = effect;
            UniqueEffectPrimaryValue = primaryValue;
            UniqueEffectSecondaryValue = secondaryValue;
            GameLogger.LogInfo(LocalizationManager.Instance.GetTextFormatted("equip_log_info_set_unique_effect", Name, effect?.name ?? "null"));
            return true;
        }

        public override string ToString()
        {
            return LocalizationManager.Instance.GetTextFormatted("equip_ui_format_tostring", Name, level, grade, Description);
        }

        protected virtual void OnValidate()
        {
            if (level < 1) level = 1;
            if (maxLevel < 1) maxLevel = 1;
            if (level > maxLevel) level = maxLevel;

            if (!CanHaveUniqueEffect && uniqueEffectSO != null)
            {
                // OnValidate는 에디터에서만 호출되므로 UnityEngine.Debug를 명시적으로 사용합니다.
                UnityEngine.Debug.LogWarning(LocalizationManager.Instance.GetText("equip_validate_warn_unique_effect_grade_mismatch"));
                uniqueEffectSO = null;
            }
        }

        public string GetUniqueEffectName()
        {
            var lm = LocalizationManager.Instance;
            if (uniqueEffectSO != null && !string.IsNullOrEmpty(uniqueEffectSO.effectNameKey))
            {
                return lm.GetText(uniqueEffectSO.effectNameKey);
            }
            return lm.GetText("equip_ui_label_no_effect");
        }

        public string GetUniqueEffectDescription()
        {
            if (uniqueEffectSO != null)
            {
                return uniqueEffectSO.GetDescription(this);
            }
            return LocalizationManager.Instance.GetText("equip_ui_label_no_effect");
        }

        public string GetUniqueEffectInfo()
        {
            if (uniqueEffectSO == null) return GetUniqueEffectName();
            return LocalizationManager.Instance.GetTextFormatted("equip_ui_format_unique_effect_info", GetUniqueEffectName(), GetUniqueEffectDescription());
        }

        public string GetAllEffectsInfo()
        {
            var sb = new StringBuilder();
            var lm = LocalizationManager.Instance;

            if (BaseEffects.Count > 0)
            {
                sb.AppendLine(lm.GetText("equip_ui_header_base_effects"));
                foreach (var effect in BaseEffects)
                {
                    sb.AppendLine(effect.ToString());
                }
            }

            if (AdditionalEffects.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine(lm.GetText("equip_ui_header_additional_effects"));
                foreach (var effect in AdditionalEffects)
                {
                    sb.AppendLine(effect.ToString());
                }
            }

            if (CanHaveUniqueEffect && uniqueEffectSO != null)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine(lm.GetText("equip_ui_header_unique_effect"));
                sb.AppendLine(GetUniqueEffectInfo());
            }

            return sb.ToString();
        }
    }
}