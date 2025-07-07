using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 장신구 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "New Accessory", menuName = "Game/Equipment/Accessory")]
    public class AccessoryData : BaseMonsterEquipment
    {
        /*
        public override void ApplyTo(ICharacter monster)
        {
            // 장신구 장착 시 특별한 로직이 있다면 여기에 추가.
            Debug.Log($"Accessory-specific Apply logic for {Name}.");
            
            base.ApplyTo(monster);
        }

        public override void RemoveFrom(ICharacter monster)
        {
            // 장신구 해제 시 특별한 로직이 있다면 여기에 추가.
            Debug.Log($"Accessory-specific Remove logic for {Name}.");
            
            base.RemoveFrom(monster);
        }
        */
    }
} 