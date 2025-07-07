using System.Collections.Generic;
using System.Linq;
using DungeonMaster.Equipment;
using UnityEngine;

namespace DungeonMaster.Managers
{
    /// <summary>
    /// 게임에 존재하는 모든 장비 ScriptableObject를 로드하고 관리하는 전역 데이터베이스.
    /// SkillManager와 유사한 싱글턴 패턴을 사용.
    /// </summary>
    public class EquipmentManager
    {
        private static EquipmentManager instance;
        public static EquipmentManager Instance => instance ?? (instance = new EquipmentManager());

        private readonly Dictionary<long, BaseMonsterEquipment> equipmentDatabase = new Dictionary<long, BaseMonsterEquipment>();

        private EquipmentManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            var allEquipments = Resources.LoadAll<BaseMonsterEquipment>("Equipment");

            if (allEquipments.Length == 0)
            {
                Debug.LogWarning("EquipmentManager: Resources/Equipment 폴더에서 장비 데이터를 찾을 수 없습니다.");
                return;
            }

            foreach (var equipment in allEquipments)
            {
                if (equipment.EquipmentId == 0)
                {
                    Debug.LogWarning($"EquipmentManager: ID가 0인 장비가 있습니다: {equipment.name}. 데이터베이스에 추가되지 않습니다.");
                    continue;
                }

                if (equipmentDatabase.ContainsKey(equipment.EquipmentId))
                {
                    Debug.LogError($"EquipmentManager: 중복된 장비 ID({equipment.EquipmentId})가 발견되었습니다. 첫 번째({equipmentDatabase[equipment.EquipmentId].name})와 두 번째({equipment.name}) 항목을 확인하세요.");
                    continue;
                }
                
                equipmentDatabase.Add(equipment.EquipmentId, equipment);
            }
            
            Debug.Log($"EquipmentManager: {equipmentDatabase.Count}개의 장비 데이터를 성공적으로 로드했습니다.");
        }

        /// <summary>
        /// ID로 장비 데이터를 가져옵니다.
        /// </summary>
        /// <param name="id">찾을 장비의 고유 ID</param>
        /// <returns>장비 데이터를 찾았으면 반환, 아니면 null</returns>
        public BaseMonsterEquipment GetEquipment(long id)
        {
            if (equipmentDatabase.TryGetValue(id, out var equipment))
            {
                return equipment;
            }
            
            Debug.LogWarning($"EquipmentManager: ID({id})에 해당하는 장비를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 모든 장비 데이터 목록을 가져옵니다.
        /// </summary>
        /// <returns>모든 장비 데이터의 리스트</returns>
        public List<BaseMonsterEquipment> GetAllEquipments()
        {
            return equipmentDatabase.Values.ToList();
        }
    }
} 