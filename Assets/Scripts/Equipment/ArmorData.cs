using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 방어구 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "New Armor", menuName = "Game/Equipment/Armor")]
    public class ArmorData : BaseMonsterEquipment
    {
        /*
        public override void ApplyTo(ICharacter monster)
        {
            // 방어구 장착 시 특별한 로직이 있다면 여기에 추가.
            Debug.Log($"Armor-specific Apply logic for {Name}.");
            
            base.ApplyTo(monster);
        }

        public override void RemoveFrom(ICharacter monster)
        {
            // 방어구 해제 시 특별한 로직이 있다면 여기에 추가.
            Debug.Log($"Armor-specific Remove logic for {Name}.");
            
            base.RemoveFrom(monster);
        }
        */
    }
} 