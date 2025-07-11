using System.Collections.Generic;
using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.DemonLord.EquipmentEffects
{
    public enum StatModifierType
    {
        Additive, // 고정값 (예: 공격력 +10)
        Multiplicative // 배율 (예: 공격력 +10%)
    }

    [CreateAssetMenu(fileName = "NewStatModifierEffect", menuName = "DungeonMaster/DemonLord/Equipment Effects/Stat Modifier")]
    public class StatModifierEffect : ScriptableObject
    {
        [Tooltip("어떤 스탯을 변경할지 선택합니다.")]
        public StatType TargetStat;
        
        [Tooltip("수정 방식을 선택합니다. (고정값 또는 배율)")]
        public StatModifierType ModifierType;

        [Tooltip("적용할 값입니다. 배율일 경우 10은 10%를 의미합니다.")]
        public float Value;
        
        public string GetDescription()
        {
            string op = ModifierType == StatModifierType.Additive ? "+" : "+";
            string suffix = ModifierType == StatModifierType.Additive ? "" : "%";
            // TODO: 지역화 필요
            return $"{TargetStat.ToString()} {op}{Value}{suffix}";
        }
        
        public void ApplyEffect(Dictionary<StatType, long> stats)
        {
            if (!stats.ContainsKey(TargetStat))
            {
                stats[TargetStat] = 0;
            }

            if (ModifierType == StatModifierType.Additive)
            {
                stats[TargetStat] += (long)Value;
            }
            else // Multiplicative
            {
                long baseValue = stats[TargetStat];
                stats[TargetStat] += (long)(baseValue * Value / 100f);
            }
        }
    }
} 