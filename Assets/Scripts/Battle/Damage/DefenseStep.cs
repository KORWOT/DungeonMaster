using DungeonMaster.Character;
using System;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 최종 방어력을 적용하여 데미지를 감소시키는 단계입니다.
    /// </summary>
    public class DefenseStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            // PenetrationStep에서 계산된 최종 방어력을 데미지에서 뺍니다.
            context.CurrentDamage -= context.FinalDefense;
        }
    }
} 