using System.Linq;
using DungeonMaster.Character;
using DungeonMaster.Data;
using DungeonMaster.Utility;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 공격자의 "속성 데미지 증가" 스탯을 적용하는 단계입니다.
    /// 스킬의 속성과 일치하는 속성 데미지만 적용됩니다.
    /// </summary>
    public class ElementalBonusDamageStep : IDamageCalculationStep
    {
        public void Calculate(DamageCalculationContext context)
        {
            var attacker = context.Attacker;
            if (attacker.Elementals == null || attacker.Elementals.Count == 0)
            {
                return;
            }

            var elementalBonusDamage = GetElementalBonusDamage(context.Attacker, context.Skill.Element);
            if (elementalBonusDamage > 0)
            {
                context.CurrentDamage = (context.CurrentDamage * (100 + elementalBonusDamage)) / 100;
            }
        }

        private long GetElementalBonusDamage(DeterministicCharacterData character, ElementType elementType)
        {
            var statType = elementType switch
            {
                ElementType.Fire => StatType.FireDamageBonus,
                ElementType.Water => StatType.WaterDamageBonus,
                ElementType.Wind => StatType.WindDamageBonus,
                ElementType.Earth => StatType.EarthDamageBonus,
                ElementType.Light => StatType.LightDamageBonus,
                ElementType.Dark => StatType.DarkDamageBonus,
                _ => (StatType?)null,
            };

            return statType.HasValue ? character.Stats.GetValueOrDefault(statType.Value, 0) : 0;
        }
    }
} 