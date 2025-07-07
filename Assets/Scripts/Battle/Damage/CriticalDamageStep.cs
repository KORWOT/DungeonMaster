using DungeonMaster.Character;
using System;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 치명타를 계산하여 데미지를 증폭시키는 단계입니다.
    /// </summary>
    public class CriticalDamageStep : IDamageCalculationStep
    {
        private readonly Random _random;

        public CriticalDamageStep(Random random)
        {
            _random = random;
        }

        public void Calculate(DamageCalculationContext context)
        {
            var critRate = context.Attacker.Stats.GetValueOrDefault(StatType.CritRate, 0); // 1% = 1
            var critDamage = context.Attacker.Stats.GetValueOrDefault(StatType.CritMultiplier, 150); // 기본 150%

            // 치명타 확률은 10000분율을 사용 (예: 25% -> 2500)
            // _random.Next(0, 10000)이 critRate보다 작으면 치명타 발생
            var randomValue = _random.Next(0, 10000);
            if (randomValue < critRate)
            {
                // 치명타 발생: 현재 데미지 * (치명타 피해량 / 100)
                context.CurrentDamage = (context.CurrentDamage * critDamage) / 100;
                
                // 치명타 발생 사실을 컨텍스트에 기록합니다.
                context.IsCritical = true;
            }
        }
    }
} 