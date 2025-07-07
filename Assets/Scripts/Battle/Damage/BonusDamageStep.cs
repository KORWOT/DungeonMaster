using DungeonMaster.Character;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 공격자의 "피해 증가량" 스탯을 적용하는 단계입니다.
    /// </summary>
    public class BonusDamageStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            var damageBonus = context.Attacker.Stats.GetValueOrDefault(StatType.DamageBonus, 0);
            if (damageBonus > 0)
            {
                context.CurrentDamage = (context.CurrentDamage * (100 + damageBonus)) / 100;
            }
        }
    }
} 