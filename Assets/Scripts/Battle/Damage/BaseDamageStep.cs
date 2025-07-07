using DungeonMaster.Character;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 공격력과 스킬 계수를 기반으로 기본 데미지를 계산하는 단계입니다.
    /// </summary>
    public class BaseDamageStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            var attack = context.Attacker.Stats.GetValueOrDefault(StatType.Attack, 0);
            long skillCoefficient = context.Skill.SkillCoefficient; // 예: 1.2배 -> 120

            // 기본 데미지 = 공격력 * (스킬 계수 / 100)
            // 정수 연산을 위해 순서를 조정: (공격력 * 스킬 계수) / 100
            context.CurrentDamage = (attack * skillCoefficient) / 100;
        }
    }
} 