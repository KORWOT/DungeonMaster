using DungeonMaster.Character;
using System.Collections.Generic;
using UnityEngine;
using DungeonMaster.Battle;
using DungeonMaster.Data;
using DungeonMaster.Utility;
using DungeonMaster.Localization;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 고유 효과 - 에픽 이상 장비에만 적용되는 특별한 효과
    /// </summary>
    [System.Serializable]
    public class UniqueEffect : EquipmentEffect, IDamageModifier
    {
        [SerializeField] private UniqueEffectType effectType;
        [SerializeField] private float primaryValue;
        [SerializeField] private float secondaryValue;
        [SerializeField] private string customDescription;
        
        // IDamageModifier 구현
        public int Priority => 100;

        public long ModifyDamage(long damage, DeterministicCharacterData attacker, DeterministicCharacterData defender, DamageContext context)
        {
            switch (effectType)
            {
                case UniqueEffectType.Berserker:
                    // 체력이 50% 이하일 때, 잃은 체력 1%당 최종 피해량 1% 증가 (최대 50%)
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

                case UniqueEffectType.Vampire:
                    // 공격 시 피해량의 일부를 체력으로 흡수 (ModifyDamage 단계가 아닌, 별도 이벤트 처리 필요)
                    // 여기서는 피해량 자체를 바꾸지 않음.
                    return damage;

                case UniqueEffectType.Assassin:
                    // 치명타 발생 시 추가 피해
                    if (context.IsCritical)
                    {
                        damage = damage * (100 + (long)primaryValue) / 100;
                    }
                    return damage;
                    
                // 다른 효과들은 데미지 계산 시점에 직접적인 영향을 주지 않으므로 기본값 반환
                default:
                    return damage;
            }
        }

        public UniqueEffectType EffectType => effectType;
        public float PrimaryValue => primaryValue;
        public float SecondaryValue => secondaryValue;
        
        public UniqueEffect(UniqueEffectType type, float primaryValue, float secondaryValue = 0f, string customDescription = "") 
            : base("", "") // 기본 이름과 설명을 비워둡니다.
        {
            this.effectType = type;
            this.primaryValue = primaryValue;
            this.secondaryValue = secondaryValue;
            
            // 이름과 설명을 지역화 시스템을 통해 생성합니다.
            this.effectName = LocalizationManager.Instance.GetText(GetKey("name"));
            this.customDescription = string.IsNullOrEmpty(customDescription) ? GenerateDescription() : customDescription;
        }
        
        public override void Apply(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                Debug.LogError($"UniqueEffect '{effectName}' 적용 실패: target 또는 Stats가 null입니다.");
                return;
            }
            
            if (equipmentLevel < 1)
            {
                Debug.LogWarning($"UniqueEffect '{effectName}' 적용 시 장비 레벨이 유효하지 않습니다: {equipmentLevel}. 1로 보정합니다.");
                equipmentLevel = 1;
            }
            
            try
            {
                float scaledPrimary = primaryValue + (equipmentLevel - 1) * (primaryValue * 0.1f);
                float scaledSecondary = secondaryValue + (equipmentLevel - 1) * (secondaryValue * 0.1f);
                
                switch (effectType)
                {
                    case UniqueEffectType.Berserker:
                        ApplyBerserkerEffect(target, scaledPrimary);
                        break;
                        
                    case UniqueEffectType.Vampire:
                        ApplyVampireEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    case UniqueEffectType.Guardian:
                        ApplyGuardianEffect(target, scaledPrimary);
                        break;
                        
                    case UniqueEffectType.Assassin:
                        ApplyAssassinEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    case UniqueEffectType.Elemental:
                        ApplyElementalEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    case UniqueEffectType.Rapid:
                        ApplyRapidEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    case UniqueEffectType.Fortress:
                        ApplyFortressEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    case UniqueEffectType.Custom:
                        ApplyCustomEffect(target, scaledPrimary, scaledSecondary);
                        break;
                        
                    default:
                        Debug.LogError($"알 수 없는 고유 효과 타입: {effectType}");
                        break;
                }
                
                Debug.Log($"{target.Name}에게 고유 효과 {effectName} 적용 (값: {scaledPrimary:F1}/{scaledSecondary:F1})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"고유 효과 '{effectName}' 적용 중 오류: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        public override void Remove(DeterministicCharacterData target, int equipmentLevel)
        {
            if (target?.Stats == null)
            {
                Debug.LogError($"UniqueEffect '{effectName}' 해제 실패: target 또는 Stats가 null입니다.");
                return;
            }
            
            if (equipmentLevel < 1)
            {
                Debug.LogWarning($"UniqueEffect '{effectName}' 해제 시 장비 레벨이 유효하지 않습니다: {equipmentLevel}. 1로 보정합니다.");
                equipmentLevel = 1;
            }
            
            try
            {
                float scaledPrimary = primaryValue + (equipmentLevel - 1) * (primaryValue * 0.1f);
                float scaledSecondary = secondaryValue + (equipmentLevel - 1) * (secondaryValue * 0.1f);
                
                switch (effectType)
                {
                    case UniqueEffectType.Berserker:
                        RemoveBerserkerEffect(target, scaledPrimary);
                        break;
                    case UniqueEffectType.Vampire:
                        RemoveVampireEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    case UniqueEffectType.Guardian:
                        RemoveGuardianEffect(target, scaledPrimary);
                        break;
                    case UniqueEffectType.Assassin:
                        RemoveAssassinEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    case UniqueEffectType.Elemental:
                        RemoveElementalEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    case UniqueEffectType.Rapid:
                        RemoveRapidEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    case UniqueEffectType.Fortress:
                        RemoveFortressEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    case UniqueEffectType.Custom:
                        RemoveCustomEffect(target, scaledPrimary, scaledSecondary);
                        break;
                    default:
                        Debug.LogError($"알 수 없는 고유 효과 타입: {effectType}");
                        break;
                }
                
                Debug.Log($"{target.Name}에게서 고유 효과 {effectName} 해제");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"고유 효과 '{effectName}' 해제 중 오류: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        #region Effect Apply Methods
        
        private void ApplyBerserkerEffect(DeterministicCharacterData target, float value)
        {
            try
            {
                target.Stats[StatType.Attack] = target.Stats.GetValueOrDefault(StatType.Attack, 0) + (long)value;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"광전사 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveBerserkerEffect(DeterministicCharacterData target, float value)
        {
            try
            {
                target.Stats[StatType.Attack] = target.Stats.GetValueOrDefault(StatType.Attack, 0) - (long)value;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"광전사 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyVampireEffect(DeterministicCharacterData target, float lifeSteal, float critLifeSteal)
        {
            try
            {
                target.Stats[StatType.LifeSteal] = target.Stats.GetValueOrDefault(StatType.LifeSteal, 0) + (long)lifeSteal;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"흡혈귀 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveVampireEffect(DeterministicCharacterData target, float lifeSteal, float critLifeSteal)
        {
            try
            {
                target.Stats[StatType.LifeSteal] = target.Stats.GetValueOrDefault(StatType.LifeSteal, 0) - (long)lifeSteal;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"흡혈귀 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyGuardianEffect(DeterministicCharacterData target, float ratio)
        {
            try
            {
                long defense = target.Stats.GetValueOrDefault(StatType.Defense, 0);
                long hpBonus = (long)(defense * ratio / 100);
                target.Stats[StatType.MaxHP] = target.Stats.GetValueOrDefault(StatType.MaxHP, 0) + hpBonus;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"수호자 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveGuardianEffect(DeterministicCharacterData target, float ratio)
        {
            try
            {
                long defense = target.Stats.GetValueOrDefault(StatType.Defense, 0);
                long hpBonus = (long)(defense * ratio / 100);
                target.Stats[StatType.MaxHP] = target.Stats.GetValueOrDefault(StatType.MaxHP, 0) - hpBonus;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"수호자 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyAssassinEffect(DeterministicCharacterData target, float critRate, float cooldownReduction)
        {
            try
            {
                target.Stats[StatType.CritRate] = target.Stats.GetValueOrDefault(StatType.CritRate, 0) + (long)critRate;
                target.Stats[StatType.CooldownReduction] = target.Stats.GetValueOrDefault(StatType.CooldownReduction, 0) + (long)cooldownReduction;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"암살자 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveAssassinEffect(DeterministicCharacterData target, float critRate, float cooldownReduction)
        {
            try
            {
                target.Stats[StatType.CritRate] = target.Stats.GetValueOrDefault(StatType.CritRate, 0) - (long)critRate;
                target.Stats[StatType.CooldownReduction] = target.Stats.GetValueOrDefault(StatType.CooldownReduction, 0) - (long)cooldownReduction;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"암살자 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyElementalEffect(DeterministicCharacterData target, float attributeDamage, float penetration)
        {
            try
            {
                // 이 효과는 모든 속성 데미지에 적용되어야 하므로, 개별적으로 처리합니다.
                target.Stats[StatType.FireDamageBonus] = target.Stats.GetValueOrDefault(StatType.FireDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.WaterDamageBonus] = target.Stats.GetValueOrDefault(StatType.WaterDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.WindDamageBonus] = target.Stats.GetValueOrDefault(StatType.WindDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.EarthDamageBonus] = target.Stats.GetValueOrDefault(StatType.EarthDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.LightDamageBonus] = target.Stats.GetValueOrDefault(StatType.LightDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.DarkDamageBonus] = target.Stats.GetValueOrDefault(StatType.DarkDamageBonus, 0) + (long)attributeDamage;
                target.Stats[StatType.PenetrationRate] = target.Stats.GetValueOrDefault(StatType.PenetrationRate, 0) + (long)penetration;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"원소술사 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveElementalEffect(DeterministicCharacterData target, float attributeDamage, float penetration)
        {
            try
            {
                target.Stats[StatType.FireDamageBonus] = target.Stats.GetValueOrDefault(StatType.FireDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.WaterDamageBonus] = target.Stats.GetValueOrDefault(StatType.WaterDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.WindDamageBonus] = target.Stats.GetValueOrDefault(StatType.WindDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.EarthDamageBonus] = target.Stats.GetValueOrDefault(StatType.EarthDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.LightDamageBonus] = target.Stats.GetValueOrDefault(StatType.LightDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.DarkDamageBonus] = target.Stats.GetValueOrDefault(StatType.DarkDamageBonus, 0) - (long)attributeDamage;
                target.Stats[StatType.PenetrationRate] = target.Stats.GetValueOrDefault(StatType.PenetrationRate, 0) - (long)penetration;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"원소술사 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyRapidEffect(DeterministicCharacterData target, float attackSpeed, float damageBonus)
        {
            try
            {
                target.Stats[StatType.AttackSpeed] = target.Stats.GetValueOrDefault(StatType.AttackSpeed, 0) + (long)attackSpeed;
                target.Stats[StatType.DamageBonus] = target.Stats.GetValueOrDefault(StatType.DamageBonus, 0) + (long)damageBonus;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"신속함 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveRapidEffect(DeterministicCharacterData target, float attackSpeed, float damageBonus)
        {
            try
            {
                target.Stats[StatType.AttackSpeed] = target.Stats.GetValueOrDefault(StatType.AttackSpeed, 0) - (long)attackSpeed;
                target.Stats[StatType.DamageBonus] = target.Stats.GetValueOrDefault(StatType.DamageBonus, 0) - (long)damageBonus;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"신속함 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyFortressEffect(DeterministicCharacterData target, float damageReduction, float reflection)
        {
            try
            {
                target.Stats[StatType.DamageReductionRate] = target.Stats.GetValueOrDefault(StatType.DamageReductionRate, 0) + (long)damageReduction;
                // 반사 데미지는 전투 시스템에서 처리
            }
            catch (System.Exception e)
            {
                Debug.LogError($"요새 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveFortressEffect(DeterministicCharacterData target, float damageReduction, float reflection)
        {
            try
            {
                target.Stats[StatType.DamageReductionRate] = target.Stats.GetValueOrDefault(StatType.DamageReductionRate, 0) - (long)damageReduction;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"요새 효과 해제 중 오류: {e.Message}");
            }
        }
        
        private void ApplyCustomEffect(DeterministicCharacterData target, float value1, float value2)
        {
            try
            {
                // 커스텀 효과 구현 (사용자 정의)
                Debug.Log($"커스텀 효과 적용: {value1}, {value2}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"커스텀 효과 적용 중 오류: {e.Message}");
            }
        }
        
        private void RemoveCustomEffect(DeterministicCharacterData target, float value1, float value2)
        {
            // 사용자 정의 효과 제거 로직
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GenerateDescription()
        {
            try
            {
                var lm = LocalizationManager.Instance;
                var descriptionKey = GetKey("desc");
                
                // 'desc_custom' 같은 키가 없을 때를 대비한 폴백
                if (!lm.ContainsKey(descriptionKey))
                {
                    return lm.GetText("unique_effect_unknown_desc");
                }
                
                return lm.GetTextFormatted(descriptionKey, primaryValue, secondaryValue);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"고유 효과 설명 생성 중 오류: {e.Message}");
                return LocalizationManager.Instance.GetText("unique_effect_error_desc");
            }
        }
        
        private string GetKey(string type)
        {
            var effectName = effectType.ToString().ToLower();
            return $"unique_effect_{effectName}_{type}";
        }
        
        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool ValidateEffect()
        {
            try
            {
                // 기본 값 범위 검증
                if (primaryValue < 0f)
                {
                    Debug.LogWarning($"고유 효과 '{effectName}'의 주 효과값이 음수입니다: {primaryValue}");
                    return false;
                }
                
                // 효과 타입별 특별 검증
                switch (effectType)
                {
                    case UniqueEffectType.Guardian:
                        if (primaryValue > 100f)
                        {
                            Debug.LogWarning($"수호자 효과의 비율이 100%를 초과합니다: {primaryValue}%");
                        }
                        break;
                        
                    case UniqueEffectType.Assassin:
                        if (primaryValue > 100f || secondaryValue > 100f)
                        {
                            Debug.LogWarning($"암살자 효과의 확률이 100%를 초과합니다: {primaryValue}%, {secondaryValue}%");
                        }
                        break;
                        
                    case UniqueEffectType.Fortress:
                        if (primaryValue > 90f)
                        {
                            Debug.LogWarning($"요새 효과의 피해 감소가 90%를 초과합니다: {primaryValue}%");
                        }
                        break;
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"고유 효과 유효성 검증 중 오류: {e.Message}");
                return false;
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 고유 효과 타입 열거형
    /// </summary>
    public enum UniqueEffectType
    {
        Berserker,  // 광전사 - 체력이 낮을수록 공격력 증가
        Vampire,    // 흡혈귀 - 흡혈 + 치명타 시 추가 흡혈
        Guardian,   // 수호자 - 방어력에 비례해 체력 증가
        Assassin,   // 암살자 - 치명타 + 치명타 시 쿨타임 감소
        Elemental,  // 원소술사 - 속성 피해 + 관통력
        Rapid,      // 신속함 - 공격속도 + 공격속도 비례 피해
        Fortress,   // 요새 - 피해 감소 + 반사 피해
        Custom      // 커스텀 - 사용자 정의 효과
    }
} 