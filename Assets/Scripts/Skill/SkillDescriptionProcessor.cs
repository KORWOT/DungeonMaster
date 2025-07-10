using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DungeonMaster.Skill
{
    /// <summary>
    /// 스킬 설명 텍스트 처리 전담 클래스 (성능 최적화 적용)
    /// SRP 원칙에 따라 SkillData에서 분리 + StringBuilder 패턴 적용
    /// </summary>
    public static class SkillDescriptionProcessor
    {
        // StringBuilder 재사용을 위한 ObjectPool
        private static readonly Stack<StringBuilder> _stringBuilderPool = new Stack<StringBuilder>();
        private const int MAX_POOLED_BUILDERS = 5;
        private const int INITIAL_CAPACITY = 512;

        // 플레이스홀더 정의 (성능을 위해 미리 정의)
        private static readonly Dictionary<string, System.Func<SkillData, int, SkillEffectData[], float, float, string>> _basicPlaceholders;
        private static readonly Dictionary<string, System.Func<SkillEffectData, string>> _effectPlaceholders;

        /// <summary>
        /// 정적 생성자 - 플레이스홀더 딕셔너리 초기화
        /// </summary>
        static SkillDescriptionProcessor()
        {
            _basicPlaceholders = new Dictionary<string, System.Func<SkillData, int, SkillEffectData[], float, float, string>>
            {
                { "{skillname}", (data, level, effects, cooldown, mana) => data.SkillName },
                { "{level}", (data, level, effects, cooldown, mana) => level.ToString() },
                { "{maxlevel}", (data, level, effects, cooldown, mana) => data.MaxLevel.ToString() },
                { "{cooldown}", (data, level, effects, cooldown, mana) => cooldown.ToString("F1") },
                { "{manacost}", (data, level, effects, cooldown, mana) => mana.ToString("F0") }
            };

            _effectPlaceholders = new Dictionary<string, System.Func<SkillEffectData, string>>
            {
                { "{damage}", effect => effect.GetEffectSummary() },
                { "{fixedvalue}", effect => effect.FixedValue().ToString("F0") },
                { "{percentvalue}", effect => effect.PercentValue().ToString("F0") },
                { "{hitcount}", effect => effect.HitCount().ToString() },
                { "{duration}", effect => effect.Duration().ToString("F1") }
            };
        }

        /// <summary>
        /// StringBuilder 풀에서 가져오기
        /// </summary>
        private static StringBuilder GetStringBuilder()
        {
            if (_stringBuilderPool.Count > 0)
            {
                var sb = _stringBuilderPool.Pop();
                sb.Clear();
                return sb;
            }
            return new StringBuilder(INITIAL_CAPACITY);
        }

        /// <summary>
        /// StringBuilder 풀에 반환
        /// </summary>
        private static void ReturnStringBuilder(StringBuilder sb)
        {
            if (_stringBuilderPool.Count < MAX_POOLED_BUILDERS && sb.Capacity <= INITIAL_CAPACITY * 4)
            {
                _stringBuilderPool.Push(sb);
            }
        }

        /// <summary>
        /// 스킬 설명 템플릿 처리 (성능 최적화 버전)
        /// </summary>
        public static string ProcessTemplate(string template, SkillData skillData, int skillLevel)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var sb = GetStringBuilder();
            try
            {
                var scaledEffects = skillData.GetScaledEffects(skillLevel);
                float scaledCooldown = skillData.GetScaledCooldown(skillLevel);
                float scaledManaCost = skillData.GetScaledManaCost(skillLevel);

                // StringBuilder에 템플릿 복사
                sb.Append(template);

                // 1. 기본 스킬 정보 치환
                ProcessBasicPlaceholders(sb, skillData, skillLevel, scaledEffects, scaledCooldown, scaledManaCost);
                
                // 2. 주요 효과 치환 (첫 번째 효과 기준)
                if (scaledEffects.Length > 0)
                {
                    ProcessPrimaryEffectPlaceholders(sb, scaledEffects[0]);
                }
                
                // 3. 다중 효과 치환 (최대 3개)
                ProcessMultipleEffectPlaceholders(sb, scaledEffects);

                return sb.ToString();
            }
            finally
            {
                ReturnStringBuilder(sb);
            }
        }

        /// <summary>
        /// 기본 플레이스홀더 치환 (성능 최적화)
        /// </summary>
        private static void ProcessBasicPlaceholders(StringBuilder sb, SkillData skillData, int skillLevel, 
            SkillEffectData[] scaledEffects, float scaledCooldown, float scaledManaCost)
        {
            foreach (var placeholder in _basicPlaceholders)
            {
                string value = placeholder.Value(skillData, skillLevel, scaledEffects, scaledCooldown, scaledManaCost);
                sb.Replace(placeholder.Key, value);
            }
        }

        /// <summary>
        /// 주요 효과 플레이스홀더 치환 (성능 최적화)
        /// </summary>
        private static void ProcessPrimaryEffectPlaceholders(StringBuilder sb, SkillEffectData primaryEffect)
        {
            // 기본 효과 플레이스홀더
            foreach (var placeholder in _effectPlaceholders)
            {
                string value = placeholder.Value(primaryEffect);
                sb.Replace(placeholder.Key, value);
            }

            // 조합된 값 처리
            ProcessCombinedValuePlaceholders(sb, primaryEffect);
            
            // 히트 표현 처리
            ProcessHitExpressionPlaceholders(sb, primaryEffect);
        }

        /// <summary>
        /// 조합된 값 플레이스홀더 치환
        /// </summary>
        private static void ProcessCombinedValuePlaceholders(StringBuilder sb, SkillEffectData effect)
        {
            string fullValue;
            if (effect.FixedValue() > 0 && effect.PercentValue() > 0)
            {
                fullValue = $"{effect.FixedValue():F0} + {effect.PercentValue():F0}%";
            }
            else if (effect.FixedValue() > 0)
            {
                fullValue = $"{effect.FixedValue():F0}";
            }
            else if (effect.PercentValue() > 0)
            {
                fullValue = $"{effect.PercentValue():F0}%";
            }
            else
            {
                fullValue = "0";
            }
            
            sb.Replace("{fullvalue}", fullValue);
        }

        /// <summary>
        /// 히트 표현 플레이스홀더 치환
        /// </summary>
        private static void ProcessHitExpressionPlaceholders(StringBuilder sb, SkillEffectData effect)
        {
            var lm = Localization.LocalizationManager.Instance;
            if (effect.HitCount() > 1)
            {
                sb.Replace("{hittext}", lm.GetTextFormatted("skill_hit_text", effect.HitCount()));
                sb.Replace("{hitsuffix}", lm.GetTextFormatted("skill_hit_suffix", effect.HitCount()));
            }
            else
            {
                sb.Replace("{hittext}", "");
                sb.Replace("{hitsuffix}", "");
            }
        }

        /// <summary>
        /// 다중 효과 플레이스홀더 치환 (성능 최적화)
        /// </summary>
        private static void ProcessMultipleEffectPlaceholders(StringBuilder sb, SkillEffectData[] scaledEffects)
        {
            for (int i = 0; i < Mathf.Min(scaledEffects.Length, 3); i++)
            {
                var effect = scaledEffects[i];
                string prefix = $"{{effect{i + 1}";
                
                sb.Replace($"{prefix}damage}}", effect.GetEffectSummary());
                sb.Replace($"{prefix}fixed}}", effect.FixedValue().ToString("F0"));
                sb.Replace($"{prefix}percent}}", effect.PercentValue().ToString("F0"));
                sb.Replace($"{prefix}hits}}", effect.HitCount().ToString());
                sb.Replace($"{prefix}duration}}", effect.Duration().ToString("F1"));
            }
        }

        /// <summary>
        /// 타겟 타입 문자열 변환
        /// </summary>
        public static string GetTargetTypeString(SkillTargetType targetType)
        {
            var key = targetType switch
            {
                SkillTargetType.Enemy => "skill_target_enemy",
                SkillTargetType.Self => "skill_target_self",
                SkillTargetType.Ally => "skill_target_ally",
                SkillTargetType.AllEnemies => "skill_target_all_enemies",
                SkillTargetType.AllAllies => "skill_target_all_allies",
                SkillTargetType.All => "skill_target_all",
                _ => "skill_target_unknown"
            };
            return Localization.LocalizationManager.Instance.GetText(key);
        }

        /// <summary>
        /// 효과 타입 문자열 변환
        /// </summary>
        public static string GetEffectTypeString(SkillEffectType effectType)
        {
            var key = effectType switch
            {
                SkillEffectType.Damage => "skill_effect_damage",
                SkillEffectType.Heal => "skill_effect_heal",
                SkillEffectType.AttackBuff => "skill_effect_attack_buff",
                SkillEffectType.DefenseBuff => "skill_effect_defense_buff",
                SkillEffectType.SpeedBuff => "skill_effect_speed_buff",
                SkillEffectType.AttackDebuff => "skill_effect_attack_debuff",
                SkillEffectType.DefenseDebuff => "skill_effect_defense_debuff",
                SkillEffectType.SpeedDebuff => "skill_effect_speed_debuff",
                SkillEffectType.Stun => "skill_effect_stun",
                SkillEffectType.Poison => "skill_effect_poison",
                SkillEffectType.Shield => "skill_effect_shield",
                _ => "skill_effect_unknown"
            };
            return Localization.LocalizationManager.Instance.GetText(key);
        }

        /// <summary>
        /// StringBuilder 풀 정리 (메모리 절약)
        /// </summary>
        public static void ClearPool()
        {
            _stringBuilderPool.Clear();
        }

        /// <summary>
        /// 스킬 데이터와 레벨을 기반으로 포맷팅된 설명 문자열을 생성합니다.
        /// </summary>
        /// <param name="skillData">설명을 생성할 스킬의 데이터</param>
        /// <param name="skillLevel">스킬 레벨 (스케일링 계산용)</param>
        /// <returns>플레이스홀더가 실제 값으로 대체된 최종 설명</returns>
        public static string Process(SkillData skillData, int skillLevel)
        {
            if (skillData == null || string.IsNullOrEmpty(skillData.DescriptionTemplate))
            {
                return "";
            }

            var sb = new StringBuilder(skillData.DescriptionTemplate);

            // 여기에 다양한 플레이스홀더를 교체하는 로직을 추가합니다.
            // 예: {SkillCoefficient}, {HitCount}, {Cooldown}, {ManaCost}, {EffectValue:Damage}, {EffectDuration:Buff} 등
            
            sb.Replace("{SkillCoefficient}", (skillData.SkillCoefficient * 100).ToString("F0"));
            sb.Replace("{HitCount}", skillData.HitCount.ToString());
            sb.Replace("{Cooldown}", skillData.GetScaledCooldown(skillLevel).ToString("F1"));
            sb.Replace("{ManaCost}", skillData.GetScaledManaCost(skillLevel).ToString());
            
            // TODO: effects 배열을 순회하며 {EffectValue:Damage} 같은 더 복잡한 플레이스홀더 처리 로직 추가 필요

            return sb.ToString();
        }
    }
} 