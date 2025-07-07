using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 방어구 장비
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterArmor", menuName = "Game/Equipment/Monster Armor")]
    public class MonsterArmor : BaseMonsterEquipment
    {
        [Header("방어구 전용 설정")]
        [SerializeField] private float baseDefenseBonus = 8f;
        [SerializeField] private float defenseLevelScaling = 1.5f;
        [SerializeField] private float baseHealthBonus = 20f;
        [SerializeField] private float healthLevelScaling = 5f;
        
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
            
            var defenseEffect = new StatModifierEffect(
                "방어력 증가", 
                StatType.Defense, 
                baseDefenseBonus, 
                defenseLevelScaling
            );
            
            var healthEffect = new StatModifierEffect(
                "체력 증가", 
                StatType.MaxHP, 
                baseHealthBonus, 
                healthLevelScaling
            );
            
            // 리플렉션을 사용하여 private 필드에 접근
            var baseEffectsField = typeof(BaseMonsterEquipment).GetField("baseEffects", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (baseEffectsField != null)
            {
                var baseEffectsList = baseEffectsField.GetValue(this) as System.Collections.Generic.List<EquipmentEffect>;
                if (baseEffectsList != null && baseEffectsList.Count == 0)
                {
                    baseEffectsList.Add(defenseEffect);
                    baseEffectsList.Add(healthEffect);
                }
            }
        }
    }
} 