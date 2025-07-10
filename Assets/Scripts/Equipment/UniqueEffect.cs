using DungeonMaster.Character;
using System.Collections.Generic;
using UnityEngine;
using DungeonMaster.Battle;
using DungeonMaster.Data;
using DungeonMaster.Utility;
using DungeonMaster.Localization;

namespace DungeonMaster.Equipment
{
    [System.Serializable]
    public class UniqueEffect : EquipmentEffect, IDamageModifier
    {
        [SerializeField] private UniqueEffectType effectType;
        [SerializeField] private ElementType elementType; // 엘리멘탈 효과를 위한 속성 타입
        [SerializeField] private float primaryValue;
        [SerializeField] private float secondaryValue;
        [SerializeField] private string customDescriptionKey; // 사용자 정의 설명은 이제 키를 저장합니다.

        public override string EffectName => LocalizationManager.Instance.GetText(GetKey("name"));
        public override string Description => string.IsNullOrEmpty(customDescriptionKey) ? GenerateDescription() : LocalizationManager.Instance.GetText(customDescriptionKey);
        
        public int Priority => 100;

        public long ModifyDamage(long damage, DeterministicCharacterData attacker, DeterministicCharacterData defender, DamageContext context)
        {
            switch (effectType)
            {
                case UniqueEffectType.Berserker:
                    long maxHp = attacker.Stats.GetValueOrDefault(StatType.MaxHP, 1);
                    if (maxHp == 0) return damage;
                    long hpPercent = (attacker.CurrentHP * 100) / maxHp; 
                    if (hpPercent <= 50) 
                    {
                        long lostHpPercent = 50 - hpPercent; 
                        long damageBonus = 100 + (lostHpPercent * (long)primaryValue / 100);
                        long maxBonus = 100 + (long)primaryValue;
                        damageBonus = System.Math.Min(damageBonus, maxBonus);
                        damage = damage * damageBonus / 100;
                    }
                    return damage;

                case UniqueEffectType.Assassin:
                    if (context.IsCritical)
                    {
                        damage = damage * (100 + (long)primaryValue) / 100;
                    }
                    return damage;
                    
                default:
                    return damage;
            }
        }

        public UniqueEffectType EffectType => effectType;
        public float PrimaryValue => primaryValue;
        public float SecondaryValue => secondaryValue;

        public UniqueEffect(UniqueEffectType type, float primaryValue, float secondaryValue = 0f, string customDescriptionKey = "", ElementType elementType = ElementType.Normal) 
            : base("", "")
        {
            this.effectType = type;
            this.elementType = elementType;
            this.primaryValue = primaryValue;
            this.secondaryValue = secondaryValue;
            this.customDescriptionKey = customDescriptionKey;
        }

        public override void Apply(DeterministicCharacterData target, int equipmentLevel)
        {
            var lm = LocalizationManager.Instance;
            if (target?.Stats == null)
            {
                GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_target_null", EffectName));
                return;
            }

            if (equipmentLevel < 1)
            {
                GameLogger.LogWarning(lm.GetTextFormatted("effect_unique_log_warn_invalid_level", EffectName, equipmentLevel));
                equipmentLevel = 1;
            }

            try
            {
                float scaledPrimary = primaryValue + (equipmentLevel - 1) * (primaryValue * 0.1f);
                float scaledSecondary = secondaryValue + (equipmentLevel - 1) * (secondaryValue * 0.1f);

                switch (effectType)
                {
                    case UniqueEffectType.Berserker:   ApplyBerserkerEffect(target, scaledPrimary); break;
                    case UniqueEffectType.Vampire:     ApplyVampireEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Guardian:    ApplyGuardianEffect(target, scaledPrimary); break;
                    case UniqueEffectType.Assassin:    ApplyAssassinEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Elemental:   ApplyElementalEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Rapid:       ApplyRapidEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Fortress:    ApplyFortressEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Custom:      ApplyCustomEffect(target, scaledPrimary, scaledSecondary); break;
                    default:
                        GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_unknown_type", effectType));
                        break;
                }
                GameLogger.LogInfo(lm.GetTextFormatted("effect_unique_log_info_applied", target.Name, EffectName, scaledPrimary, scaledSecondary));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_apply_exception", EffectName, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }

        public override void Remove(DeterministicCharacterData target, int equipmentLevel)
        {
            var lm = LocalizationManager.Instance;
            if (target?.Stats == null)
            {
                GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_target_null", EffectName));
                return;
            }

            if (equipmentLevel < 1)
            {
                GameLogger.LogWarning(lm.GetTextFormatted("effect_unique_log_warn_invalid_level", EffectName, equipmentLevel));
                equipmentLevel = 1;
            }

            try
            {
                float scaledPrimary = primaryValue + (equipmentLevel - 1) * (primaryValue * 0.1f);
                float scaledSecondary = secondaryValue + (equipmentLevel - 1) * (secondaryValue * 0.1f);

                switch (effectType)
                {
                    case UniqueEffectType.Berserker:   RemoveBerserkerEffect(target, scaledPrimary); break;
                    case UniqueEffectType.Vampire:     RemoveVampireEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Guardian:    RemoveGuardianEffect(target, scaledPrimary); break;
                    case UniqueEffectType.Assassin:    RemoveAssassinEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Elemental:   RemoveElementalEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Rapid:       RemoveRapidEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Fortress:    RemoveFortressEffect(target, scaledPrimary, scaledSecondary); break;
                    case UniqueEffectType.Custom:      RemoveCustomEffect(target, scaledPrimary, scaledSecondary); break;
                    default:
                        GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_unknown_type", effectType));
                        break;
                }
                GameLogger.LogInfo(lm.GetTextFormatted("effect_unique_log_info_removed", target.Name, EffectName));
            }
            catch (System.Exception e)
            {
                GameLogger.LogError(lm.GetTextFormatted("effect_unique_log_error_remove_exception", EffectName, e.Message));
                UnityEngine.Debug.LogException(e);
            }
        }
        
        #region Effect Apply/Remove Methods
        private void HandleEffect(System.Action action, string errorKey)
        {
            try { action(); }
            catch (System.Exception e) { GameLogger.LogError(LocalizationManager.Instance.GetTextFormatted(errorKey, e.Message)); }
        }

        private void ApplyBerserkerEffect(DeterministicCharacterData target, float value) => HandleEffect(() => target.Stats[StatType.Attack] += (long)value, "effect_unique_log_error_berserker_apply");
        private void RemoveBerserkerEffect(DeterministicCharacterData target, float value) => HandleEffect(() => target.Stats[StatType.Attack] -= (long)value, "effect_unique_log_error_berserker_remove");
        private void ApplyVampireEffect(DeterministicCharacterData target, float lifeSteal, float critLifeSteal) => HandleEffect(() => target.Stats[StatType.LifeSteal] += (long)lifeSteal, "effect_unique_log_error_vampire_apply");
        private void RemoveVampireEffect(DeterministicCharacterData target, float lifeSteal, float critLifeSteal) => HandleEffect(() => target.Stats[StatType.LifeSteal] -= (long)lifeSteal, "effect_unique_log_error_vampire_remove");
        private void ApplyGuardianEffect(DeterministicCharacterData target, float ratio) => HandleEffect(() => target.Stats[StatType.MaxHP] += (long)(target.Stats.GetValueOrDefault(StatType.Defense, 0) * ratio / 100), "effect_unique_log_error_guardian_apply");
        private void RemoveGuardianEffect(DeterministicCharacterData target, float ratio) => HandleEffect(() => target.Stats[StatType.MaxHP] -= (long)(target.Stats.GetValueOrDefault(StatType.Defense, 0) * ratio / 100), "effect_unique_log_error_guardian_remove");
        private void ApplyAssassinEffect(DeterministicCharacterData target, float critRate, float cooldownReduction) => HandleEffect(() => target.Stats[StatType.CritRate] += (long)critRate, "effect_unique_log_error_assassin_apply");
        private void RemoveAssassinEffect(DeterministicCharacterData target, float critRate, float cooldownReduction) => HandleEffect(() => target.Stats[StatType.CritRate] -= (long)critRate, "effect_unique_log_error_assassin_remove");
        
        private void ApplyElementalEffect(DeterministicCharacterData target, float attributeDamage, float penetration)
        {
            StatType? statType = GetElementalStatType(elementType);
            if (statType.HasValue)
            {
                HandleEffect(() => {
                    target.Stats[statType.Value] += (long)attributeDamage;
                    target.Stats[StatType.Penetration] += (long)penetration;
                }, "effect_unique_log_error_elemental_apply");
            }
        }

        private void RemoveElementalEffect(DeterministicCharacterData target, float attributeDamage, float penetration)
        {
            StatType? statType = GetElementalStatType(elementType);
            if (statType.HasValue)
            {
                HandleEffect(() => {
                    target.Stats[statType.Value] -= (long)attributeDamage;
                    target.Stats[StatType.Penetration] -= (long)penetration;
                }, "effect_unique_log_error_elemental_remove");
            }
        }

        private void ApplyRapidEffect(DeterministicCharacterData target, float attackSpeed, float damageBonus) => HandleEffect(() => target.Stats[StatType.AttackSpeed] += (long)attackSpeed, "effect_unique_log_error_rapid_apply");
        private void RemoveRapidEffect(DeterministicCharacterData target, float attackSpeed, float damageBonus) => HandleEffect(() => target.Stats[StatType.AttackSpeed] -= (long)attackSpeed, "effect_unique_log_error_rapid_remove");
        private void ApplyFortressEffect(DeterministicCharacterData target, float damageReduction, float reflection) => HandleEffect(() => target.Stats[StatType.DamageReduction] += (long)damageReduction, "effect_unique_log_error_fortress_apply");
        private void RemoveFortressEffect(DeterministicCharacterData target, float damageReduction, float reflection) => HandleEffect(() => target.Stats[StatType.DamageReduction] -= (long)damageReduction, "effect_unique_log_error_fortress_remove");
        private void ApplyCustomEffect(DeterministicCharacterData target, float value1, float value2) => HandleEffect(() => { /* Custom logic */ }, "effect_unique_log_error_custom_apply");
        private void RemoveCustomEffect(DeterministicCharacterData target, float value1, float value2) => HandleEffect(() => { /* Custom logic */ }, "effect_unique_log_error_custom_remove");
        #endregion

        private string GenerateDescription()
        {
            var lm = LocalizationManager.Instance;
            if (effectType == UniqueEffectType.Elemental)
            {
                string elementNameKey = $"element_{elementType.ToString().ToLower()}";
                // string elementName = lm.GetText(elementNameKey, elementType.ToString()); // 폴백을 위해 기본 이름 제공 -> GetText 오버로드 없음
                string elementName = lm.GetText(elementNameKey) ?? elementType.ToString();
                return lm.GetTextFormatted("effect_unique_elemental_specific_desc", elementName, primaryValue, secondaryValue);
            }

            string descKey = GetKey("desc");
            return lm.GetTextFormatted(descKey, primaryValue, secondaryValue);
        }

        private string GetKey(string type)
        {
            return $"effect_unique_{effectType.ToString().ToLower()}_{type}";
        }

        private StatType? GetElementalStatType(ElementType type)
        {
            switch (type)
            {
                case ElementType.Fire: return StatType.FireDamageBonus;
                case ElementType.Water: return StatType.WaterDamageBonus;
                case ElementType.Wind: return StatType.WindDamageBonus;
                case ElementType.Earth: return StatType.EarthDamageBonus;
                case ElementType.Light: return StatType.LightDamageBonus;
                case ElementType.Dark: return StatType.DarkDamageBonus;
                default:
                    GameLogger.LogWarning($"Unsupported element type for elemental effect: {type}");
                    return null;
            }
        }

        public bool ValidateEffect()
        {
            var lm = LocalizationManager.Instance;
            if (effectType == default && primaryValue == 0 && secondaryValue == 0)
            {
                GameLogger.LogError(lm.GetText("effect_unique_validate_error_missing_type"));
                return false;
            }
            // 각 효과 타입에 따른 값 유효성 검사 추가 가능
            return true;
        }
    }

    public enum UniqueEffectType
    {
        Berserker,  
        Vampire,    
        Guardian,   
        Assassin,   
        Elemental,  
        Rapid,      
        Fortress,   
        Custom      
    }
} 