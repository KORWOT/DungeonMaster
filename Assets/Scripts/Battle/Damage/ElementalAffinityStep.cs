using DungeonMaster.Data;

namespace DungeonMaster.Battle.Damage
{
    /// <summary>
    /// 속성 상성에 따라 데미지를 조절하는 단계입니다.
    /// </summary>
    public class ElementalAffinityStep : IDamageCalculationStep
    {
        private readonly ElementalAffinityTable _affinityTable;

        public ElementalAffinityStep(ElementalAffinityTable affinityTable)
        {
            _affinityTable = affinityTable;
        }

        public void Calculate(DamageCalculationContext context)
        {
            var attackerElement = context.Attacker.Element;
            var defenderElement = context.Defender.Element;

            // 데미지 배율 가져오기 (예: 1.5배 -> 150)
            long multiplier = _affinityTable.GetMultiplier(attackerElement, defenderElement);

            // 데미지 조절: 현재 데미지 * (배율 / 100)
            context.CurrentDamage = (context.CurrentDamage * multiplier) / 100;
        }
    }
} 