using DungeonMaster.Character;
using System;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 공격자의 방어 관통 스탯을 적용하여 최종 방어력을 계산하는 단계입니다.
    /// 이 단계는 데미지를 직접 수정하지 않고, 컨텍스트의 FinalDefense 값을 설정합니다.
    /// </summary>
    public class PenetrationStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            long targetDefense = context.Defender.Stats.GetValueOrDefault(StatType.Defense, 0);

            long penetrationRate = context.Attacker.Stats.GetValueOrDefault(StatType.PenetrationRate, 0);
            long penetrationValue = context.Attacker.Stats.GetValueOrDefault(StatType.Penetration, 0);
            
            // 비율 방어 관통 적용
            long finalDefense = targetDefense * (100 - penetrationRate) / 100;
            
            // 고정 방어 관통 적용
            finalDefense = Math.Max(0, finalDefense - penetrationValue);

            context.FinalDefense = finalDefense;
        }
    }
} 