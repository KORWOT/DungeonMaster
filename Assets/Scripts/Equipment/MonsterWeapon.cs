using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 무기 장비
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterWeapon", menuName = "Game/Equipment/Monster Weapon")]
    public class MonsterWeapon : BaseMonsterEquipment
    {
        [Header("무기 전용 설정")]
        [SerializeField] private float baseAttackBonus = 10f;
        [SerializeField] private float attackLevelScaling = 2f;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            #if UNITY_EDITOR
            // 무기는 항상 공격력 증가 기본 효과를 가짐
            InitializeBaseEffects();
            #endif
        }
        
        private void Awake()
        {
            InitializeBaseEffects();
        }
        
        private void InitializeBaseEffects()
        {
            // 기본 효과가 이미 있으면 스킵
            if (BaseEffects.Count > 0) return;
            
            // baseEffects 필드에 직접 접근하여 추가
            var attackEffect = new StatModifierEffect(
                "공격력 증가", 
                StatType.Attack, 
                baseAttackBonus, 
                attackLevelScaling
            );
            
            // 리플렉션을 사용하여 private 필드에 접근
            var baseEffectsField = typeof(BaseMonsterEquipment).GetField("baseEffects", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (baseEffectsField != null)
            {
                var baseEffectsList = baseEffectsField.GetValue(this) as System.Collections.Generic.List<EquipmentEffect>;
                if (baseEffectsList != null && baseEffectsList.Count == 0)
                {
                    baseEffectsList.Add(attackEffect);
                }
            }
        }
    }
} 