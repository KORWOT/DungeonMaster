using System.Collections.Generic;
using DungeonMaster.Data;
using UnityEngine;

namespace DungeonMaster.DemonLord
{
    [CreateAssetMenu(fileName = "NewPermanentEquipment", menuName = "DungeonMaster/DemonLord/Permanent Equipment Blueprint")]
    public class DemonLordPermanentEquipmentBlueprint : ScriptableObject
    {
        [Header("ID 및 이름")]
        public string Guid; // 이 장비 인스턴스의 고유 ID
        public string NameKey;
        [TextArea]
        public string DescriptionKey;
        public Sprite Icon;

        [Header("장착 정보")]
        public DemonLordEquipmentSlot Slot;

        [Header("효과 목록")]
        [Tooltip("이 장비가 제공하는 모든 효과의 목록입니다. ScriptableObject로 된 효과들을 여기에 추가하세요.")]
        public List<ScriptableObject> Effects = new List<ScriptableObject>();
    }
} 