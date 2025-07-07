using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 무기 데이터.
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Equipment/Weapon")]
    public class WeaponData : BaseMonsterEquipment
    {
        // ... 기존의 다른 필드나 메서드는 그대로 둡니다 ...
        /*
        public override void ApplyTo(ICharacter monster)
        {
            // 무기 장착 시 특별한 로직이 있다면 여기에 추가.
            // 없다면 base 호출만으로 충분합니다.
            Debug.Log($"Weapon-specific Apply logic for {Name}.");
            
            base.ApplyTo(monster);
        }

        public override void RemoveFrom(ICharacter monster)
        {
            // 무기 해제 시 특별한 로직이 있다면 여기에 추가.
            Debug.Log($"Weapon-specific Remove logic for {Name}.");
            
            base.RemoveFrom(monster);
        }
        */
    }
} 