using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Data;  // DeterministicCharacterData를 위해 추가
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 장비 기본 ScriptableObject
    /// 작업원칙에 따라 데이터 계층과 뷰 계층을 모두 지원
    /// </summary>
    public abstract class BaseMonsterEquipment : ScriptableObject, IMonsterEquipment
    {
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
        [SerializeField] private UniqueEffect uniqueEffect;
        
        /// <summary>
        /// 지정된 레벨에 대한 모든 스탯 보너스를 계산하여 반환합니다.
        /// 이 메서드는 결정론적 데이터 생성에 사용됩니다.
        /// </summary>
        /// <param name="atLevel">계산의 기준이 될 레벨</param>
        /// <returns>스탯 타입과 보너스 값의 딕셔너리</returns>
        public Dictionary<StatType, long> GetAllStatBonuses(int atLevel)
        {
            var totalBonuses = new Dictionary<StatType, long>();

            // 기본 효과 합산
            foreach (var effect in BaseEffects)
            {
                if (effect is StatModifierEffect statEffect)
                {
                    long value = (long)(statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, atLevel - 1)));
                    if (totalBonuses.ContainsKey(statEffect.StatType))
                    {
                        totalBonuses[statEffect.StatType] += value;
                    }
                    else
                    {
                        totalBonuses[statEffect.StatType] = value;
                    }
                }
            }

            // 추가 효과 합산
            foreach (var effect in AdditionalEffects)
            {
                if (effect is StatModifierEffect statEffect)
                {
                    long value = (long)(statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, atLevel - 1)));
                    if (totalBonuses.ContainsKey(statEffect.StatType))
                    {
                        totalBonuses[statEffect.StatType] += value;
                    }
                    else
                    {
                        totalBonuses[statEffect.StatType] = value;
                    }
                }
            }
            
            return totalBonuses;
        }
        
        // 프로퍼티
        public long EquipmentId => equipmentId;
        public string Name => string.IsNullOrEmpty(equipmentName) ? "알 수 없는 장비" : equipmentName;
        public string Description => string.IsNullOrEmpty(description) ? "설명이 없습니다." : description;
        public MonsterEquipmentType EquipmentType => equipmentType;
        public EquipmentGrade Grade => grade;
        public int Level => level;
        public int MaxLevel => maxLevel;
        public List<EquipmentEffect> BaseEffects => baseEffects ?? new List<EquipmentEffect>();
        public List<EquipmentEffect> AdditionalEffects => additionalEffects ?? new List<EquipmentEffect>();
        public UniqueEffect UniqueEffect => uniqueEffect;
        
        /// <summary>
        /// 현재 등급에서 권장되는 추가 효과 개수 (참고용, 실제 제한 없음)
        /// 향후 밸런싱을 위해 제한을 걸 수도 있는 가이드라인
        /// </summary>
        public int RecommendedAdditionalEffects => grade switch
        {
            EquipmentGrade.Normal => 0,
            EquipmentGrade.Magic => 1,
            EquipmentGrade.Rare => 3,
            EquipmentGrade.Epic => 6, // 고유 효과 1개 + 추가 효과 5개 권장
            EquipmentGrade.Legendary => 10, // 고유 효과 1개 + 추가 효과 9개 권장
            _ => 0
        };
        
        /// <summary>
        /// 고유 효과 사용 가능 여부
        /// </summary>
        public bool CanHaveUniqueEffect => grade >= EquipmentGrade.Epic;
        
        /// <summary>
        /// 장비 효과 적용 - DeterministicCharacterData에 적용 (데이터 계층)
        /// 작업원칙에 따라 데이터 계층과 뷰 계층을 분리
        /// </summary>
        public virtual void ApplyTo(DeterministicCharacterData characterData)
        {
            if (characterData?.Stats == null)
            {
                Debug.LogError($"{Name} 장비 적용 실패: 대상 캐릭터 데이터가 null이거나 Stats가 없습니다.");
                return;
            }
            
            if (!ValidateEquipment())
            {
                Debug.LogError($"{Name} 장비 적용 실패: 장비 유효성 검증 실패");
                return;
            }
            
            try
            {
                // 기본 효과 적용
                ApplyEffectList(BaseEffects, characterData, "기본 효과");
                
                // 등급에 따른 추가 효과 적용 (제한 없음)
                ApplyEffectList(AdditionalEffects, characterData, "추가 효과");
                
                // 고유 효과 적용 (에픽 이상)
                if (CanHaveUniqueEffect && uniqueEffect != null)
                {
                    uniqueEffect.Apply(characterData, level);
                }
                
                Debug.Log($"{characterData.Name}에게 {Name} (레벨{level}, {grade}) 장착 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{Name} 장비 적용 중 오류 발생: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// 장비 효과 해제 - DeterministicCharacterData에서만 해제
        /// </summary>
        public virtual void RemoveFrom(DeterministicCharacterData characterData)
        {
            if (characterData?.Stats == null)
            {
                Debug.LogError($"{Name} 장비 해제 실패: 대상 캐릭터 데이터가 null이거나 Stats가 없습니다.");
                return;
            }
            
            try
            {
                // 기본 효과 해제
                RemoveEffectList(BaseEffects, characterData, "기본 효과");
                
                // 추가 효과 해제 (제한 없음)
                RemoveEffectList(AdditionalEffects, characterData, "추가 효과");
                
                // 고유 효과 해제
                if (CanHaveUniqueEffect && uniqueEffect != null)
                {
                    uniqueEffect.Remove(characterData, level);
                }
                
                Debug.Log($"{characterData.Name}에게서 {Name} 해제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{Name} 장비 해제 중 오류 발생: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// 장비 효과 적용 - ICharacter에 적용 (뷰 계층 호환성)
        /// 기존 코드와의 호환성을 위해 유지
        /// </summary>
        public virtual void ApplyTo(ICharacter monster)
        {
            if (monster?.Stats == null)
            {
                Debug.LogError($"{Name} 장비 적용 실패: 대상 몬스터가 null이거나 Stats가 없습니다.");
                return;
            }
            
            if (!ValidateEquipment())
            {
                Debug.LogError($"{Name} 장비 적용 실패: 장비 유효성 검증 실패");
                return;
            }
            
            try
            {
                // 기본 효과 적용
                ApplyEffectListToCharacter(BaseEffects, monster, "기본 효과");
                
                // 등급에 따른 추가 효과 적용 (제한 없음)
                ApplyEffectListToCharacter(AdditionalEffects, monster, "추가 효과");
                
                // 고유 효과는 ICharacter에서 지원하지 않음 (데이터 계층 전용)
                if (CanHaveUniqueEffect && uniqueEffect != null)
                {
                    Debug.LogWarning($"{Name}의 고유 효과는 ICharacter에 적용할 수 없습니다. DeterministicCharacterData를 사용하세요.");
                }
                
                Debug.Log($"{monster.Name}에게 {Name} (레벨{level}, {grade}) 장착 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{Name} 장비 적용 중 오류 발생: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// 장비 효과 해제 - ICharacter에서 해제 (뷰 계층 호환성)
        /// </summary>
        public virtual void RemoveFrom(ICharacter monster)
        {
            if (monster?.Stats == null)
            {
                Debug.LogError($"{Name} 장비 해제 실패: 대상 몬스터가 null이거나 Stats가 없습니다.");
                return;
            }
            
            try
            {
                // 기본 효과 해제
                RemoveEffectListFromCharacter(BaseEffects, monster, "기본 효과");
                
                // 추가 효과 해제 (제한 없음)
                RemoveEffectListFromCharacter(AdditionalEffects, monster, "추가 효과");
                
                // 고유 효과는 ICharacter에서 지원하지 않음
                
                Debug.Log($"{monster.Name}에게서 {Name} 해제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{Name} 장비 해제 중 오류 발생: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// 효과 리스트 적용 헬퍼 메서드 - DeterministicCharacterData용
        /// </summary>
        private void ApplyEffectList(List<EquipmentEffect> effects, DeterministicCharacterData characterData, string effectType)
        {
            if (effects == null) return;
            
            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    Debug.LogWarning($"{Name}의 {effectType} 중 null 효과가 발견되었습니다.");
                    continue;
                }
                
                try
                {
                    effect.Apply(characterData, level);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{Name}의 {effectType} '{effect.EffectName}' 적용 중 오류: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 효과 리스트 해제 헬퍼 메서드 - DeterministicCharacterData용
        /// </summary>
        private void RemoveEffectList(List<EquipmentEffect> effects, DeterministicCharacterData characterData, string effectType)
        {
            if (effects == null) return;
            
            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    Debug.LogWarning($"{Name}의 {effectType} 중 null 효과가 발견되었습니다.");
                    continue;
                }
                
                try
                {
                    effect.Remove(characterData, level);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{Name}의 {effectType} '{effect.EffectName}' 해제 중 오류: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 효과 리스트 적용 헬퍼 메서드 - ICharacter용
        /// </summary>
        private void ApplyEffectListToCharacter(List<EquipmentEffect> effects, ICharacter monster, string effectType)
        {
            if (effects == null) return;
            
            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    Debug.LogWarning($"{Name}의 {effectType} 중 null 효과가 발견되었습니다.");
                    continue;
                }
                
                try
                {
                    // ICharacter는 EquipmentEffect를 직접 지원하지 않으므로, 
                    // StatModifierEffect만 처리
                    if (effect is StatModifierEffect statEffect)
                    {
                        float totalValue = statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, level - 1));
                        if (monster.Stats.ContainsKey(statEffect.StatType))
                        {
                            monster.Stats[statEffect.StatType] += (long)totalValue;
                        }
                        else
                        {
                            monster.Stats[statEffect.StatType] = (long)totalValue;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{Name}의 {effectType} '{effect.EffectName}' 적용 중 오류: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 효과 리스트 해제 헬퍼 메서드 - ICharacter용
        /// </summary>
        private void RemoveEffectListFromCharacter(List<EquipmentEffect> effects, ICharacter monster, string effectType)
        {
            if (effects == null) return;
            
            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    Debug.LogWarning($"{Name}의 {effectType} 중 null 효과가 발견되었습니다.");
                    continue;
                }
                
                try
                {
                    if (effect is StatModifierEffect statEffect)
                    {
                        float totalValue = statEffect.BaseValue + (statEffect.LevelScaling * Mathf.Max(0, level - 1));
                        if (monster.Stats.ContainsKey(statEffect.StatType))
                        {
                            monster.Stats[statEffect.StatType] -= (long)totalValue;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"{Name}의 {effectType} '{effect.EffectName}' 해제 중 오류: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 장비 유효성 검증
        /// </summary>
        private bool ValidateEquipment()
        {
            if (string.IsNullOrEmpty(equipmentName))
            {
                Debug.LogError("장비 이름이 설정되지 않았습니다.");
                return false;
            }
            
            if (level < 1 || level > maxLevel)
            {
                Debug.LogError($"장비 레벨이 유효하지 않습니다: {level} (1~{maxLevel} 범위)");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 레벨 업그레이드 가능 여부 확인
        /// </summary>
        public virtual bool CanUpgrade()
        {
            return level < maxLevel;
        }
        
        /// <summary>
        /// 레벨 업그레이드
        /// </summary>
        public virtual void UpgradeLevel()
        {
            if (!CanUpgrade())
            {
                Debug.LogWarning($"{Name} 레벨 업그레이드 실패: 최대 레벨에 도달했습니다. (현재: {level}/{maxLevel})");
                return;
            }
            
            level++;
            Debug.Log($"{Name} 레벨 업그레이드 완료: {level - 1} → {level}");
        }
        
        /// <summary>
        /// 등급 업그레이드
        /// </summary>
        public virtual void UpgradeGrade()
        {
            if (grade >= EquipmentGrade.Legendary)
            {
                Debug.LogWarning($"{Name} 등급 업그레이드 실패: 이미 최고 등급입니다. (현재: {grade})");
                return;
            }
            
            var oldGrade = grade;
            grade = (EquipmentGrade)((int)grade + 1);
            
            Debug.Log($"{Name} 등급 업그레이드 완료: {oldGrade} → {grade}");
        }
        
        /// <summary>
        /// 고유 효과 설정
        /// </summary>
        public virtual bool SetUniqueEffect(UniqueEffect effect)
        {
            if (!CanHaveUniqueEffect)
            {
                Debug.LogWarning($"{Name} 고유 효과 설정 실패: {grade} 등급은 고유 효과를 가질 수 없습니다. (Epic 이상 필요)");
                return false;
            }
            
            uniqueEffect = effect;
            Debug.Log($"{Name}에 고유 효과 설정: {effect?.EffectName ?? "없음"}");
            return true;
        }
        
        /// <summary>
        /// 장비 정보를 문자열로 반환
        /// </summary>
        public override string ToString()
        {
            var uniqueText = CanHaveUniqueEffect && uniqueEffect != null ? $" + {uniqueEffect.EffectName}" : "";
            var additionalText = AdditionalEffects.Count > 0 ? $" (+{AdditionalEffects.Count}개 추가효과)" : "";
            
            return $"[{grade}] {Name} Lv.{level}/{maxLevel} ({EquipmentType}){additionalText}{uniqueText}";
        }
        
        /// <summary>
        /// Inspector에서 값이 변경될 때 호출 (에디터 전용)
        /// </summary>
        protected virtual void OnValidate()
        {
            // 레벨 범위 검증
            if (level < 1) level = 1;
            if (level > maxLevel) level = maxLevel;
            if (maxLevel < 1) maxLevel = 1;
            
            // 등급에 따른 고유 효과 검증
            if (!CanHaveUniqueEffect && uniqueEffect != null)
            {
                Debug.LogWarning($"{name}: {grade} 등급은 고유 효과를 가질 수 없어 제거됩니다.");
                uniqueEffect = null;
            }
        }
        
        /// <summary>
        /// 고유 효과 이름 반환
        /// </summary>
        public string GetUniqueEffectName()
        {
            return uniqueEffect?.EffectName ?? "없음";
        }
        
        /// <summary>
        /// 고유 효과 설명 반환
        /// </summary>
        public string GetUniqueEffectDescription()
        {
            return uniqueEffect?.Description ?? "고유 효과가 없습니다.";
        }
        
        /// <summary>
        /// 고유 효과 상세 정보 반환
        /// </summary>
        public string GetUniqueEffectInfo()
        {
            if (uniqueEffect == null)
                return "고유 효과 없음";
            
            return $"고유 효과: {uniqueEffect.EffectName}\n설명: {uniqueEffect.Description}";
        }
        
        /// <summary>
        /// 모든 효과 정보 반환
        /// </summary>
        public string GetAllEffectsInfo()
        {
            var info = new System.Text.StringBuilder();
            
            // 기본 효과들
            if (baseEffects != null && baseEffects.Count > 0)
            {
                info.AppendLine("기본 효과:");
                foreach (var effect in baseEffects)
                {
                    if (effect != null)
                    {
                        info.AppendLine($"  - {effect.EffectName}: {effect.Description}");
                    }
                }
            }
            
            // 고유 효과
            if (uniqueEffect != null)
            {
                info.AppendLine($"고유 효과: {uniqueEffect.EffectName}");
                info.AppendLine($"  설명: {uniqueEffect.Description}");
            }
            
            return info.ToString();
        }
    }
}