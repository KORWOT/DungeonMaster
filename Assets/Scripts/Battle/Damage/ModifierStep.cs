using System.Collections.Generic;
using System.Linq;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 공격자와 방어자의 모든 IDamageModifier를 적용하는 단계입니다.
    /// 파이프라인의 마지막에 위치하여 최종 데미지를 보정합니다.
    /// </summary>
    public class ModifierStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            var allModifiers = new List<IDamageModifier>();
            if (context.Attacker.DamageModifiers != null)
            {
                allModifiers.AddRange(context.Attacker.DamageModifiers);
            }
            if (context.Defender.DamageModifiers != null)
            {
                allModifiers.AddRange(context.Defender.DamageModifiers);
            }

            if (allModifiers.Count == 0)
            {
                return;
            }

            // DamageCalculationContext의 정보를 기반으로 레거시 DamageContext를 생성합니다.
            // TODO: 향후 DamageContext를 DamageCalculationContext로 통합하는 리팩토링을 고려할 수 있습니다.
            var legacyContext = new DamageContext
            {
                SkillName = context.Skill.Name,
                SkillMultiplier = context.Skill.SkillCoefficient,
                // 파이프라인을 통해 전달된 실제 치명타 발생 여부를 반영합니다.
                IsCritical = context.IsCritical 
            };

            // 우선순위(Priority)에 따라 정렬하여 실행합니다.
            foreach (var modifier in allModifiers.OrderBy(m => m.Priority))
            {
                context.CurrentDamage = modifier.ModifyDamage(context.CurrentDamage, context.Attacker, context.Defender, legacyContext);
            }
        }
    }
} 