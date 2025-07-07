using System.Collections.Generic;
using DungeonMaster.Character;
using UnityEngine;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 몬스터 장비 관리자 (몬스터별 개별 인스턴스)
    /// </summary>
    [System.Serializable]
    public class MonsterEquipmentManager
    {
        [Header("장착된 장비")]
        [SerializeField] private BaseMonsterEquipment weapon;
        [SerializeField] private BaseMonsterEquipment armor;
        [SerializeField] private BaseMonsterEquipment accessory;
        
        private ICharacter owner;
        
        // 프로퍼티
        public BaseMonsterEquipment Weapon => weapon;
        public BaseMonsterEquipment Armor => armor;
        public BaseMonsterEquipment Accessory => accessory;
        
        public MonsterEquipmentManager(ICharacter owner)
        {
            this.owner = owner ?? throw new System.ArgumentNullException(nameof(owner), "Owner는 null일 수 없습니다.");
        }
        
        /// <summary>
        /// 장비 장착
        /// </summary>
        public bool EquipItem(BaseMonsterEquipment equipment)
        {
            if (equipment == null)
            {
                Debug.LogError("null 장비를 장착할 수 없습니다.");
                return false;
            }
            
            if (owner?.Stats == null)
            {
                Debug.LogError("소유자가 null이거나 Stats가 없어 장비를 장착할 수 없습니다.");
                return false;
            }
            
            try
            {
                // 같은 타입의 기존 장비 해제
                UnequipByType(equipment.EquipmentType);
                
                // 새 장비 장착
                switch (equipment.EquipmentType)
                {
                    case MonsterEquipmentType.Weapon:
                        weapon = equipment;
                        break;
                    case MonsterEquipmentType.Armor:
                        armor = equipment;
                        break;
                    case MonsterEquipmentType.Accessory:
                        accessory = equipment;
                        break;
                    default:
                        Debug.LogError($"알 수 없는 장비 타입: {equipment.EquipmentType}");
                        return false;
                }
                
                // 효과 적용
                equipment.ApplyTo(owner);
                Debug.Log($"{owner.Name}이(가) {equipment.Name} 장착");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{owner?.Name ?? "알 수 없는 소유자"}의 장비 장착 중 오류: {e.Message}");
                Debug.LogException(e);
                return false;
            }
        }
        
        /// <summary>
        /// 장비 해제
        /// </summary>
        public bool UnequipItem(MonsterEquipmentType equipmentType)
        {
            var equipment = GetEquippedItem(equipmentType);
            if (equipment == null) 
            {
                Debug.LogWarning($"{equipmentType} 슬롯에 장착된 장비가 없습니다.");
                return false;
            }
            
            if (owner?.Stats == null)
            {
                Debug.LogError("소유자가 null이거나 Stats가 없어 장비를 해제할 수 없습니다.");
                return false;
            }
            
            try
            {
                // 효과 해제
                equipment.RemoveFrom(owner);
                
                // 슬롯에서 제거
                switch (equipmentType)
                {
                    case MonsterEquipmentType.Weapon:
                        weapon = null;
                        break;
                    case MonsterEquipmentType.Armor:
                        armor = null;
                        break;
                    case MonsterEquipmentType.Accessory:
                        accessory = null;
                        break;
                    default:
                        Debug.LogError($"알 수 없는 장비 타입: {equipmentType}");
                        return false;
                }
                
                Debug.Log($"{owner.Name}이(가) {equipment.Name} 해제");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{owner?.Name ?? "알 수 없는 소유자"}의 장비 해제 중 오류: {e.Message}");
                Debug.LogException(e);
                return false;
            }
        }
        
        /// <summary>
        /// 타입별 장비 해제
        /// </summary>
        private void UnequipByType(MonsterEquipmentType equipmentType)
        {
            UnequipItem(equipmentType);
        }
        
        /// <summary>
        /// 장착된 장비 가져오기
        /// </summary>
        public BaseMonsterEquipment GetEquippedItem(MonsterEquipmentType equipmentType)
        {
            return equipmentType switch
            {
                MonsterEquipmentType.Weapon => weapon,
                MonsterEquipmentType.Armor => armor,
                MonsterEquipmentType.Accessory => accessory,
                _ => null
            };
        }
        
        /// <summary>
        /// 모든 장비 해제
        /// </summary>
        public void UnequipAll()
        {
            if (owner?.Stats == null)
            {
                Debug.LogError("소유자가 null이거나 Stats가 없어 장비를 해제할 수 없습니다.");
                return;
            }
            
            try
            {
                UnequipItem(MonsterEquipmentType.Weapon);
                UnequipItem(MonsterEquipmentType.Armor);
                UnequipItem(MonsterEquipmentType.Accessory);
                Debug.Log($"{owner.Name}의 모든 장비 해제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{owner?.Name ?? "알 수 없는 소유자"}의 모든 장비 해제 중 오류: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// 장착된 모든 장비 목록
        /// </summary>
        public List<BaseMonsterEquipment> GetAllEquippedItems()
        {
            var items = new List<BaseMonsterEquipment>();
            
            try
            {
                if (weapon != null) items.Add(weapon);
                if (armor != null) items.Add(armor);
                if (accessory != null) items.Add(accessory);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"장착된 장비 목록 조회 중 오류: {e.Message}");
            }
            
            return items;
        }
        
        // TODO: 장비 강화 시스템은 나중에 구현 예정
        
        /// <summary>
        /// 장비 상태 정보
        /// </summary>
        public string GetEquipmentInfo()
        {
            try
            {
                var info = $"{owner?.Name ?? "알 수 없는 소유자"}의 장비 현황:\n";
                
                if (weapon != null)
                    info += $"무기: {weapon}\n";
                else
                    info += "무기: 없음\n";
                    
                if (armor != null)
                    info += $"방어구: {armor}\n";
                else
                    info += "방어구: 없음\n";
                    
                if (accessory != null)
                    info += $"악세사리: {accessory}\n";
                else
                    info += "악세사리: 없음\n";
                    
                return info;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"장비 정보 조회 중 오류: {e.Message}");
                return $"{owner?.Name ?? "알 수 없는 소유자"}의 장비 정보 (오류 발생)";
            }
        }
        
        /// <summary>
        /// 소유자 변경 (안전한 이전)
        /// </summary>
        public void TransferOwnership(ICharacter newOwner)
        {
            if (newOwner?.Stats == null)
            {
                Debug.LogError("새 소유자가 null이거나 Stats가 없습니다.");
                return;
            }
            
            try
            {
                // 기존 소유자에서 모든 효과 해제
                if (owner?.Stats != null)
                {
                    UnequipAll();
                }
                
                // 새 소유자 설정
                owner = newOwner;
                
                // 새 소유자에게 모든 효과 적용
                var equippedItems = GetAllEquippedItems();
                foreach (var item in equippedItems)
                {
                    item?.ApplyTo(owner);
                }
                
                Debug.Log($"장비 소유권이 {newOwner.Name}으로 이전되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"장비 소유권 이전 중 오류: {e.Message}");
                Debug.LogException(e);
            }
        }
    }

} 