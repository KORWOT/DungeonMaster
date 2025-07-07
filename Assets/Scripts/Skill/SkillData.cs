using System;
using System.Collections.Generic;
using DungeonMaster.Character;
using DungeonMaster.Shared.Scaling;
using UnityEngine;
using System.Text;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬 타입
    /// </summary>
    public enum SkillType
    {
        Active,   // 액티브 스킬
        Passive   // 패시브 스킬
    }

    /// <summary>
    /// 스킬 효과 데이터
    /// </summary>
    [Serializable]
    public struct SkillEffectData
    {
        public SkillEffectType EffectType;
        public List<float> Values;
        public long BuffId;
    }

    /// <summary>
    /// 스킬 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private long skillId; // 스킬의 고유 숫자 ID
        [SerializeField] private string skillName;
        [SerializeField, TextArea(3, 5)] private string descriptionTemplate;
        [SerializeField] private Sprite icon;
        [SerializeField] private SkillType skillType;
        [SerializeField] private SkillGrade skillGrade = SkillGrade.D;  // 스킬 등급 추가

        [Header("데미지 공식")]
        [SerializeField] private ElementType elementType = ElementType.Normal; // 스킬 속성
        [SerializeField, Range(0, 1000)] private long skillCoefficient = 100;     // 스킬 계수 (100 = 1.0배)
        [SerializeField, Range(1, 10)] private int hitCount = 1;                     // 히트 수

        [Header("사용 조건")]
        [SerializeField] private int requiredLevel = 0;      // 필요 레벨 (0이면 제한 없음)
        [SerializeField] private float cooldown = 0f;        // 쿨다운 (초)
        [SerializeField] private int manaCost = 0;           // 마나 소모량 (0이면 소모 없음)
        [SerializeField] private SkillTargetType targetType;      // 대상 타입

        [Header("스킬 효과")]
        [SerializeField] private SkillEffectData[] effects;  // 스킬 효과들

        [Header("레벨 시스템")]
        [SerializeField] private int maxLevel = 10;                    // 최대 스킬 레벨
        
        [Header("쿨다운 & 마나 개별 스케일링")]
        [SerializeField] private IndividualScaling cooldownScaling = new IndividualScaling(ScalingType.None);
        [SerializeField] private IndividualScaling manaCostScaling = new IndividualScaling(ScalingType.None);

        [Header("애니메이션/사운드")]
        [SerializeField] private string animationTrigger;    // 애니메이션 트리거
        [SerializeField] private AudioClip skillSound;       // 스킬 사운드
        [SerializeField] private GameObject effectPrefab;    // 이펙트 프리팹

        // 프로퍼티들
        public long SkillId => skillId; // 스킬 ID 프로퍼티 추가
        public string SkillName => skillName;
        public string Name => skillName;  // 기존 필드 재사용
        public string DescriptionTemplate => descriptionTemplate;
        public Sprite Icon => icon;
        public SkillType Type => skillType;
        public SkillGrade Grade => skillGrade;  // 스킬 등급 프로퍼티 추가
        public ElementType Element => elementType;
        public long SkillCoefficient => skillCoefficient;
        public int HitCount => hitCount;
        public int RequiredLevel => requiredLevel;
        public float Cooldown => cooldown;
        public int ManaCost => manaCost;
        public SkillTargetType TargetType => targetType;
        public SkillEffectData[] Effects => effects;
        public int MaxLevel => maxLevel;
        public IndividualScaling CooldownScaling => cooldownScaling;
        public IndividualScaling ManaCostScaling => manaCostScaling;
        public string AnimationTrigger => animationTrigger;
        public AudioClip SkillSound => skillSound;
        public GameObject EffectPrefab => effectPrefab;

        /// <summary>
        /// 스킬 레벨에 따른 효과값 계산 (개별 스케일링 시스템) - 캐싱 적용
        /// </summary>
        public SkillEffectData[] GetScaledEffects(int skillLevel)
        {
            // 스킬 레벨 제한
            skillLevel = Mathf.Clamp(skillLevel, 1, maxLevel);
            
            return SkillDataCache.GetOrComputeScaledEffects(this, skillLevel, () =>
            {
                var scaledEffects = new SkillEffectData[effects.Length];

                for (int i = 0; i < effects.Length; i++)
                {
                    // 각 효과별 개별 스케일링 적용
                    scaledEffects[i] = effects[i].GetScaledEffect(skillLevel, maxLevel);
                }

                return scaledEffects;
            });
        }

        /// <summary>
        /// 레벨에 따른 스케일링된 쿨다운 계산 - 캐싱 적용
        /// </summary>
        public float GetScaledCooldown(int skillLevel)
        {
            skillLevel = Mathf.Clamp(skillLevel, 1, maxLevel);
            
            return SkillDataCache.GetOrComputeScaledValue(this, skillLevel, "Cooldown", () =>
            {
                float scaledCooldown = cooldown + cooldownScaling.CalculateScaling(skillLevel, maxLevel);
                return Mathf.Max(0.1f, scaledCooldown); // 최소 0.1초 보장
            });
        }

        /// <summary>
        /// 레벨에 따른 스케일링된 마나 소모량 계산 - 캐싱 적용
        /// </summary>
        public float GetScaledManaCost(int skillLevel)
        {
            skillLevel = Mathf.Clamp(skillLevel, 1, maxLevel);
            
            return SkillDataCache.GetOrComputeScaledValue(this, skillLevel, "ManaCost", () =>
            {
                float scaledManaCost = manaCost + manaCostScaling.CalculateScaling(skillLevel, maxLevel);
                return Mathf.Max(0f, scaledManaCost); // 음수 방지
            });
        }

        /// <summary>
        /// 전체 스킬 레벨업 미리보기 (쿨다운, 마나, 모든 효과 포함) - 성능 최적화 및 캐싱 적용
        /// </summary>
        public string GetCompletePreview(int currentLevel)
        {
            if (currentLevel >= maxLevel)
                return "최대 레벨입니다.";

            return SkillDataCache.GetOrComputeDescription(this, currentLevel, "CompletePreview", () =>
            {
                int nextLevel = currentLevel + 1;
                var sb = new StringBuilder(256);
                
                // 제목
                sb.Append("레벨 ").Append(currentLevel).Append(" → ").Append(nextLevel)
                  .AppendLine(":").AppendLine();

                // 쿨다운 변화 (0이 아닐 때만 표시)
                float currentCooldown = GetScaledCooldown(currentLevel);
                float nextCooldown = GetScaledCooldown(nextLevel);
                if (cooldown > 0 && Mathf.Abs(currentCooldown - nextCooldown) > 0.01f)
                {
                    sb.Append("쿨다운: ").Append(currentCooldown.ToString("F1")).Append("초 → ")
                      .Append(nextCooldown.ToString("F1")).AppendLine("초");
                }

                // 마나 소모량 변화 (0이 아닐 때만 표시)
                float currentMana = GetScaledManaCost(currentLevel);
                float nextMana = GetScaledManaCost(nextLevel);
                if (manaCost > 0 && Mathf.Abs(currentMana - nextMana) > 0.01f)
                {
                    sb.Append("마나 소모: ").Append(currentMana.ToString("F0")).Append(" → ")
                      .Append(nextMana.ToString("F0")).AppendLine();
                }

                // 각 효과별 변화
                for (int i = 0; i < effects.Length; i++)
                {
                    string effectPreview = effects[i].GetLevelComparison(currentLevel, nextLevel, maxLevel);
                    if (!string.IsNullOrWhiteSpace(effectPreview))
                    {
                        sb.Append(effectPreview);
                    }
                }

                return sb.ToString();
            });
        }

        /// <summary>
        /// 스킬 사용 가능 여부 확인 (조건부 체크)
        /// </summary>
        public bool CanUse(ICharacter caster, ICharacter target = null)
        {
            // 레벨 체크 (0이면 제한 없음)
            if (requiredLevel > 0 && caster.Stats[StatType.Level] < requiredLevel)
            {
                Debug.Log($"스킬 '{skillName}' 사용 불가: 필요 레벨 {requiredLevel}, 현재 레벨 {caster.Stats[StatType.Level]}");
                return false;
            }

            // 마나 체크 (현재 마나 시스템 없음 - 나중에 활성화 예정)
            // if (manaCost > 0 && caster.Stats.ContainsKey(StatType.MP))
            // {
            //     float scaledManaCost = GetScaledManaCost(1);
            //     if (caster.Stats[StatType.MP] < scaledManaCost)
            //     {
            //         Debug.Log($"스킬 '{skillName}' 사용 불가: 필요 마나 {scaledManaCost:F0}, 현재 마나 {caster.Stats[StatType.MP]:F0}");
            //         return false;
            //     }
            // }

            // 대상 체크
            if (targetType == SkillTargetType.Enemy && target == null)
            {
                Debug.Log($"스킬 '{skillName}' 사용 불가: 대상이 필요함");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 스킬 설명 (레벨 적용 + 동적 템플릿) - 성능 최적화 및 캐싱 적용
        /// </summary>
        public string GetDetailedDescription(int skillLevel)
        {
            skillLevel = Mathf.Clamp(skillLevel, 1, maxLevel);
            
            return SkillDataCache.GetOrComputeDescription(this, skillLevel, "DetailedDescription", () =>
            {
                var sb = new StringBuilder(512);
                
                // 제목
                sb.Append(skillName).Append(" (Lv.").Append(skillLevel).AppendLine(")");
                
                // 동적 템플릿 처리된 설명
                string processedDescription = ProcessDescriptionTemplate(descriptionTemplate, skillLevel);
                sb.Append(processedDescription).AppendLine().AppendLine();
                
                // 스킬 스펙 (스케일링 적용)
                sb.Append("대상: ").AppendLine(SkillDescriptionProcessor.GetTargetTypeString(targetType));
                
                // 쿨다운 표시 (0이 아닐 때만)
                float scaledCooldown = GetScaledCooldown(skillLevel);
                if (scaledCooldown > 0)
                {
                    sb.Append("쿨다운: ").Append(scaledCooldown.ToString("F1")).AppendLine("초");
                }
                
                // 마나 소모 표시 (0이 아닐 때만)
                float scaledManaCost = GetScaledManaCost(skillLevel);
                if (scaledManaCost > 0)
                {
                    sb.Append("마나 소모: ").AppendLine(scaledManaCost.ToString("F0"));
                }
                
                // 필요 레벨 표시 (0이 아닐 때만)
                if (requiredLevel > 0)
                {
                    sb.Append("필요 레벨: ").AppendLine(requiredLevel.ToString());
                }
                
                // 레벨별 효과
                var scaledEffects = GetScaledEffects(skillLevel);
                sb.AppendLine().AppendLine("[효과]");
                
                foreach (var effect in scaledEffects)
                {
                    sb.Append("• ").Append(SkillDescriptionProcessor.GetEffectTypeString(effect.Type()))
                      .Append(": ").Append(effect.GetEffectSummary());
                      
                    if (effect.Duration() > 0)
                    {
                        sb.Append(" (").Append(effect.Duration().ToString("F1")).Append("초)");
                    }
                    sb.AppendLine();
                }
                
                return sb.ToString();
            });
        }

        /// <summary>
        /// 설명 템플릿 처리 (동적 값 치환)
        /// SRP 원칙에 따라 SkillDescriptionProcessor로 위임
        /// </summary>
        private string ProcessDescriptionTemplate(string template, int skillLevel)
        {
            return SkillDescriptionProcessor.ProcessTemplate(template, this, skillLevel);
        }

        /// <summary>
        /// 간단한 스킬 설명 (템플릿 적용)
        /// </summary>
        public string GetSimpleDescription(int skillLevel)
        {
            return ProcessDescriptionTemplate(descriptionTemplate, skillLevel);
        }

        /// <summary>
        /// 스킬 레벨업 미리보기 (간단 버전)
        /// </summary>
        public string GetLevelUpPreview(int currentLevel)
        {
            if (currentLevel >= maxLevel)
                return "최대 레벨에 도달했습니다.";
                
            return GetCompletePreview(currentLevel);
        }

        /// <summary>
        /// Unity Inspector에서 유효성 검증 (캐시 무효화 포함)
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(skillName))
                skillName = name;

            // 필요 레벨은 0 이상 (0이면 제한 없음)
            if (requiredLevel < 0)
                requiredLevel = 0;

            // 쿨다운은 0 이상 (0이면 즉시 사용 가능)
            if (cooldown < 0)
                cooldown = 0;

            // 마나 소모는 0 이상 (0이면 소모 없음)
            if (manaCost < 0)
                manaCost = 0;

            // 레벨 시스템 검증
            if (maxLevel < 1)
                maxLevel = 1;

            // 개별 스케일링 검증
            ValidateIndividualScaling(ref cooldownScaling);
            ValidateIndividualScaling(ref manaCostScaling);

            // 데이터 변경 시 캐시 무효화 (에디터에서만)
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SkillDataCache.InvalidateSkillCache(this);
            }
#endif
        }

        /// <summary>
        /// 개별 스케일링 유효성 검증 (Strategy 패턴 적용)
        /// </summary>
        private void ValidateIndividualScaling(ref IndividualScaling scaling)
        {
            // Strategy를 통한 설정 검증
            if (!scaling.ValidateConfig())
            {
                // 유효하지 않은 설정이면 기본 설정으로 초기화
                Debug.LogWarning($"스킬 '{skillName}'의 {scaling.scalingType} 스케일링 설정이 유효하지 않습니다. 기본 설정으로 초기화합니다.");
                scaling.ResetToDefault();
            }

            // 커스텀 스케일링의 경우 maxLevel에 맞춰 조정
            if (scaling.scalingType == ScalingType.Custom)
            {
                var customMultipliers = scaling.CustomMultipliers;
                if (customMultipliers == null || customMultipliers.Length != maxLevel)
                {
                    // maxLevel에 맞춰 배열 크기 조정
                    var newMultipliers = new float[maxLevel];
                    for (int i = 0; i < maxLevel; i++)
                    {
                        if (customMultipliers != null && i < customMultipliers.Length)
                        {
                            newMultipliers[i] = customMultipliers[i];
                        }
                        else
                        {
                            newMultipliers[i] = 1f + (i * 0.2f); // 기본값: 20%씩 증가
                        }
                    }
                    scaling.CustomMultipliers = newMultipliers;
                }
            }
        }

        /// <summary>
        /// 스킬 레벨에 맞춰 동적으로 생성된 최종 설명을 반환합니다.
        /// </summary>
        public string GetFormattedDescription(int skillLevel)
        {
            return SkillDescriptionProcessor.Process(this, skillLevel);
        }
    }
} 