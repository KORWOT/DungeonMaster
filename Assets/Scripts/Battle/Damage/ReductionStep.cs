using DungeonMaster.Character;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 방어자의 보호 및 피해 감소 관련 스탯을 적용하는 단계입니다.
    /// </summary>
    public class ReductionStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            long protectionRate = context.Defender.Stats.GetValueOrDefault(StatType.ProtectionRate, 0);
            long damageReductionRate = context.Defender.Stats.GetValueOrDefault(StatType.DamageReductionRate, 0);
            long damageReductionValue = context.Defender.Stats.GetValueOrDefault(StatType.DamageReduction, 0);

            long currentDamage = context.CurrentDamage;

            // 비율 감소 적용
            if (protectionRate > 0)
            {
                currentDamage = currentDamage * (100 - protectionRate) / 100;
            }
            if (damageReductionRate > 0)
            {
                currentDamage = currentDamage * (100 - damageReductionRate) / 100;
            }

            // 고정 감소 적용
            currentDamage -= damageReductionValue;

            context.CurrentDamage = currentDamage;
        }
    }
} 