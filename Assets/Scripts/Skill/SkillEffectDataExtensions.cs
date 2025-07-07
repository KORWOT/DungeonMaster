using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Skill
{
    public static class SkillEffectDataExtensions
    {
        // ✅ 불변성 보장 - 새 인스턴스 생성
        public static SkillEffectData GetScaledEffect(this SkillEffectData effect, int skillLevel, int maxLevel)
        {
            return new SkillEffectData
            {
                EffectType = effect.EffectType,
                BuffId = effect.BuffId,
                Values = effect.Values?.Select(v => v * (1 + (skillLevel - 1) * 0.1f)).ToList() ?? new List<float>()
            };
        }

        // ✅ 방어적 프로그래밍 - 안전한 접근
        public static float GetValue(this SkillEffectData effect, int index, float defaultValue = 0f)
        {
            return effect.Values?.Count > index ? effect.Values[index] : defaultValue;
        }

        // ✅ 편의 메서드들 - 가정 최소화
        public static string GetEffectSummary(this SkillEffectData effect) 
            => effect.GetValue(0).ToString("F0");
        
        public static float FixedValue(this SkillEffectData effect) 
            => effect.GetValue(0);
        
        public static float PercentValue(this SkillEffectData effect) 
            => effect.GetValue(1);
        
        public static int HitCount(this SkillEffectData effect) 
            => (int)effect.GetValue(2, 1f);
        
        public static float Duration(this SkillEffectData effect) 
            => effect.GetValue(3);
        
        public static SkillEffectType Type(this SkillEffectData effect) 
            => effect.EffectType;

        public static string GetLevelComparison(this SkillEffectData effect, int currentLevel, int nextLevel, int maxLevel)
        {
            var current = effect.GetScaledEffect(currentLevel, maxLevel);
            var next = effect.GetScaledEffect(nextLevel, maxLevel);
            return $"{current.GetEffectSummary()} → {next.GetEffectSummary()}";
        }
    }
} 