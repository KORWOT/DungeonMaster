using UnityEngine;
using System.Collections.Generic;
using DungeonMaster.Character;

namespace DungeonMaster.Equipment
{
    /// <summary>
    /// 전역 몬스터 장비 관리자 (장비 목록 관리)
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalMonsterEquipmentManager", menuName = "Game/Equipment/Global Equipment Manager")]
    public class GlobalMonsterEquipmentManager : ScriptableObject
    {
        [Header("장비 풀")]
        [SerializeField] private List<BaseMonsterEquipment> weaponPool = new List<BaseMonsterEquipment>();
        [SerializeField] private List<BaseMonsterEquipment> armorPool = new List<BaseMonsterEquipment>();
        [SerializeField] private List<BaseMonsterEquipment> accessoryPool = new List<BaseMonsterEquipment>();
        
        /// <summary>
        /// 타입별 장비 풀 가져오기
        /// </summary>
        public List<BaseMonsterEquipment> GetPoolByType(MonsterEquipmentType type)
        {
            return type switch
            {
                MonsterEquipmentType.Weapon => weaponPool,
                MonsterEquipmentType.Armor => armorPool,
                MonsterEquipmentType.Accessory => accessoryPool,
                _ => new List<BaseMonsterEquipment>()
            };
        }
        
        /// <summary>
        /// 모든 장비 가져오기
        /// </summary>
        public List<BaseMonsterEquipment> GetAllEquipment()
        {
            var allEquipment = new List<BaseMonsterEquipment>();
            allEquipment.AddRange(weaponPool);
            allEquipment.AddRange(armorPool);
            allEquipment.AddRange(accessoryPool);
            return allEquipment;
        }
        
        /// <summary>
        /// 특정 장비 찾기
        /// </summary>
        public BaseMonsterEquipment FindEquipment(string equipmentName)
        {
            var allEquipment = GetAllEquipment();
            return allEquipment.Find(equipment => equipment.Name == equipmentName);
        }
        
        /// <summary>
        /// 장비 풀에 장비 추가
        /// </summary>
        public void AddEquipment(BaseMonsterEquipment equipment)
        {
            if (equipment == null) return;
            
            var pool = GetPoolByType(equipment.EquipmentType);
            if (!pool.Contains(equipment))
            {
                pool.Add(equipment);
                Debug.Log($"{equipment.Name} 장비가 {equipment.EquipmentType} 풀에 추가되었습니다.");
            }
        }
        
        /// <summary>
        /// 장비 풀에서 장비 제거
        /// </summary>
        public void RemoveEquipment(BaseMonsterEquipment equipment)
        {
            if (equipment == null) return;
            
            var pool = GetPoolByType(equipment.EquipmentType);
            if (pool.Remove(equipment))
            {
                Debug.Log($"{equipment.Name} 장비가 {equipment.EquipmentType} 풀에서 제거되었습니다.");
            }
        }
        
        /// <summary>
        /// 장비 풀 유효성 검증
        /// </summary>
        public bool ValidateEquipmentPools()
        {
            try
            {
                bool isValid = true;
                
                if (weaponPool == null || weaponPool.Count == 0)
                {
                    Debug.LogWarning("무기 풀이 비어있습니다.");
                    isValid = false;
                }
                
                if (armorPool == null || armorPool.Count == 0)
                {
                    Debug.LogWarning("방어구 풀이 비어있습니다.");
                    isValid = false;
                }
                
                if (accessoryPool == null || accessoryPool.Count == 0)
                {
                    Debug.LogWarning("악세사리 풀이 비어있습니다.");
                    isValid = false;
                }
                
                // null 아이템 검사
                int nullCount = 0;
                nullCount += weaponPool?.RemoveAll(x => x == null) ?? 0;
                nullCount += armorPool?.RemoveAll(x => x == null) ?? 0;
                nullCount += accessoryPool?.RemoveAll(x => x == null) ?? 0;
                
                if (nullCount > 0)
                {
                    Debug.LogWarning($"장비 풀에서 {nullCount}개의 null 아이템이 제거되었습니다.");
                }
                
                return isValid;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"장비 풀 유효성 검증 중 오류: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 장비 풀 정보 가져오기
        /// </summary>
        public string GetPoolInfo()
        {
            return $"장비 풀 현황:\n" +
                   $"- 무기: {weaponPool?.Count ?? 0}개\n" +
                   $"- 방어구: {armorPool?.Count ?? 0}개\n" +
                   $"- 악세사리: {accessoryPool?.Count ?? 0}개\n" +
                   $"- 총 장비 수: {GetAllEquipment().Count}개";
        }
        
        /// <summary>
        /// 에디터에서 유효성 검사
        /// </summary>
        private void OnValidate()
        {
            // null 아이템 제거
            weaponPool?.RemoveAll(x => x == null);
            armorPool?.RemoveAll(x => x == null);
            accessoryPool?.RemoveAll(x => x == null);
        }
    }
} 