using DungeonMaster.Character;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 악세사리 장비
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterAccessory", menuName = "Game/Equipment/Monster Accessory")]
    public class MonsterAccessory : BaseMonsterEquipment
    {
        [Header("악세사리 전용 설정")]
        [SerializeField] private AccessoryType accessoryType;
        [SerializeField] private float baseBonusValue = 5f;
        [SerializeField] private float levelScaling = 1f;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            #if UNITY_EDITOR
            InitializeBaseEffects();
            #endif
        }
        
        private void Awake()
        {
            InitializeBaseEffects();
        }
        
        private void InitializeBaseEffects()
        {
            if (BaseEffects.Count > 0) return;
            
            // 리플렉션을 사용하여 private 필드에 접근
            var baseEffectsField = typeof(BaseMonsterEquipment).GetField("baseEffects", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (baseEffectsField != null)
            {
                var baseEffectsList = baseEffectsField.GetValue(this) as System.Collections.Generic.List<EquipmentEffect>;
                if (baseEffectsList != null && baseEffectsList.Count == 0)
                {
                    SetupBaseEffectsByType(baseEffectsList);
                }
            }
        }
        
        private void SetupBaseEffectsByType(System.Collections.Generic.List<EquipmentEffect> effectsList)
        {
            switch (accessoryType)
            {
                case AccessoryType.Ring:
                    // 반지: 공격력과 치명타 확률 증가
                    effectsList.Add(new StatModifierEffect("공격력 증가", StatType.Attack, baseBonusValue, levelScaling));
                    effectsList.Add(new StatModifierEffect("치명타 확률 증가", StatType.CritRate, baseBonusValue, levelScaling));
                    break;
                    
                case AccessoryType.Necklace:
                    // 목걸이: 방어력과 체력 증가
                    effectsList.Add(new StatModifierEffect("방어력 증가", StatType.Defense, baseBonusValue, levelScaling));
                    effectsList.Add(new StatModifierEffect("체력 증가", StatType.MaxHP, baseBonusValue * 4, levelScaling * 2));
                    break;
                    
                case AccessoryType.Bracelet:
                    // 팔찌: 공격속도와 쿨타임 감소
                    effectsList.Add(new StatModifierEffect("공격속도 증가", StatType.AttackSpeed, baseBonusValue, levelScaling));
                    effectsList.Add(new StatModifierEffect("쿨타임 감소", StatType.CooldownReduction, baseBonusValue, levelScaling));
                    break;
                    
                case AccessoryType.Charm:
                    // 부적: 치명타와 회피율 증가
                    effectsList.Add(new StatModifierEffect("치명타 확률 증가", StatType.CritRate, baseBonusValue, levelScaling));
                    effectsList.Add(new StatModifierEffect("회피율 증가", StatType.EvasionRate, baseBonusValue, levelScaling));
                    break;
            }
        }
    }
    
    /// <summary>
    /// 악세사리 세부 타입
    /// </summary>
    public enum AccessoryType
    {
        Ring,       // 반지
        Necklace,   // 목걸이
        Bracelet,   // 팔찌
        Charm       // 부적
    }
} 